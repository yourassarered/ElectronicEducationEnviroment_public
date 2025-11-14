using ElectronicEducationEnviroment.Admin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace ElectronicEducationEnviroment.Teacher
{
    public partial class AnswerCheckWindow : Window
    {
        private readonly ElectronicEducationEnviromentEntities _context;
        private readonly int _assignmentId;
        private List<AssignmentAnswer> _answers;
        private int _currentIndex = 0;

        public AnswerCheckWindow(int assignmentId)
        {
            InitializeComponent();
            _context = App.Context;
            _assignmentId = assignmentId;

            Loaded += AnswerCheckWindow_Loaded;
        }

        private void AnswerCheckWindow_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAnswers();
        }

        private void LoadAnswers()
        {
            _answers = _context.AssignmentAnswers
                .Where(a => a.IdAssignment == _assignmentId && a.Grade == null)
                .ToList();

            if (!_answers.Any())
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    MessageBox.Show("Нет непроверенных ответов.");
                    Close();
                }), DispatcherPriority.ApplicationIdle);

                return;
            }

            ShowCurrentAnswer();
        }

        private void ShowCurrentAnswer()
        {
            var current = _answers[_currentIndex];
            var user = _context.Users.FirstOrDefault(u => u.IdUser == current.IdStudent);

            StudentNameText.Text = $"Студент: {user?.FirstName} {user?.LastName}";
            AnswerText.Text = current.AnswerText ?? "(Студент не ввёл текст ответа)";

            FilesPanel.Children.Clear();

            var attachedFiles = _context.AssignmentFiles
                .Where(f => f.IdAssignmentAnswer == current.IdAnswer)
                .ToList();

            if (attachedFiles.Any())
            {
                foreach (var file in attachedFiles)
                {
                    try
                    {
                        // Создаём текстовое поле с названием файла
                        var tb = new System.Windows.Controls.TextBlock
                        {
                            Text = file.OriginalFileName,
                            Margin = new Thickness(0, 2, 0, 2)
                        };
                        FilesPanel.Children.Add(tb);

                        // Попытка открыть файл через стандартное приложение Windows
                        if (System.IO.File.Exists(file.FilePath))
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = file.FilePath,
                                UseShellExecute = true
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        // Если файл не открылся, показываем сообщение в отладку
                        Console.WriteLine($"Не удалось открыть файл {file.FilePath}: {ex.Message}");
                    }
                }
            }
            else
            {
                // Нет файлов
                var noFilesText = new System.Windows.Controls.TextBlock
                {
                    Text = "Файл не прикреплён",
                    Foreground = System.Windows.Media.Brushes.Gray
                };
                FilesPanel.Children.Add(noFilesText);
            }
        }

        private void Rate_Click(object sender, RoutedEventArgs e)
        {
            if (_currentIndex >= _answers.Count) return;

            var btn = (FrameworkElement)sender;
            int grade = int.Parse(btn.Tag.ToString());

            _answers[_currentIndex].IdTeacher = App.IdUser;
            _answers[_currentIndex].Grade = grade.ToString();
            _context.SaveChanges();

            _currentIndex++;

            if (_currentIndex >= _answers.Count)
            {
                MessageBox.Show("Все ответы проверены!");
                Close();
            }
            else
            {
                ShowCurrentAnswer();
            }
        }
    }
}
