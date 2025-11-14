using ElectronicEducationEnviroment.Admin;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ElectronicEducationEnviroment.Student
{
    public partial class TaskViewPage : Page
    {
        private readonly ElectronicEducationEnviromentEntities _context;
        private readonly int _assignmentId;
        private AssignmentAnswer _existingAnswer;

        // Список временно прикреплённых файлов
        private List<string> _attachedFiles = new List<string>();

        // Корневая папка для всех ответов
        private readonly string RootFolder = @"C:\AssignmentFiles";

        public TaskViewPage(int assignmentId)
        {
            InitializeComponent();
            _context = App.Context;
            _assignmentId = assignmentId;

            // Создаём корневую папку, если нет
            if (!Directory.Exists(RootFolder))
                Directory.CreateDirectory(RootFolder);

            LoadAssignment();
        }

        private void LoadAssignment()
        {
            var assignment = _context.Assignments.FirstOrDefault(a => a.IdAssignment == _assignmentId);
            if (assignment == null)
            {
                MessageBox.Show("Задание не найдено");
                return;
            }

            TitleText.Text = assignment.AssignmentTitle;
            DescriptionText.Text = assignment.AssignmentText ?? "(Нет описания)";

            _existingAnswer = _context.AssignmentAnswers
                .FirstOrDefault(a => a.IdAssignment == _assignmentId && a.IdStudent == App.IdUser);

            if (_existingAnswer != null)
            {
                AnswerBox.Text = _existingAnswer.AnswerText;
                SubmitBtn.Content = "Сохранить изменения";

                var existingFiles = _context.AssignmentFiles
                    .Where(f => f.IdAssignmentAnswer == _existingAnswer.IdAssignment)
                    .ToList();

                foreach (var f in existingFiles)
                {
                    _attachedFiles.Add(f.FilePath);
                    AddFileToUI(f.FilePath, f.OriginalFileName);
                }
            }
        }

        private void AttachFileBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == true)
            {
                string filePath = ofd.FileName;
                string fileName = Path.GetFileName(filePath);

                if (_attachedFiles.Contains(filePath))
                {
                    MessageBox.Show("Этот файл уже прикреплён.");
                    return;
                }

                _attachedFiles.Add(filePath);
                AddFileToUI(filePath, fileName);
            }
        }

        private void AddFileToUI(string filePath, string displayName)
        {
            var sp = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };
            var tb = new TextBlock { Text = displayName, VerticalAlignment = VerticalAlignment.Center };
            var btnRemove = new Button
            {
                Content = "✖",
                Width = 25,
                Height = 25,
                Margin = new Thickness(5, 0, 0, 0),
                Tag = filePath
            };
            btnRemove.Click += BtnRemoveFile_Click;

            sp.Children.Add(tb);
            sp.Children.Add(btnRemove);

            FilesPanel.Children.Add(sp);
        }

        private void BtnRemoveFile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string path)
            {
                _attachedFiles.Remove(path);

                // Удаляем из UI
                var sp = (StackPanel)btn.Parent;
                FilesPanel.Children.Remove(sp);
            }
        }

        private void SubmitBtn_Click(object sender, RoutedEventArgs e)
        {
            var text = AnswerBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(text) && !_attachedFiles.Any())
            {
                MessageBox.Show("Введите ответ или прикрепите хотя бы один файл.");
                return;
            }

            try
            {
                AssignmentAnswer answer;
                if (_existingAnswer != null)
                {
                    answer = _existingAnswer;
                    answer.AnswerText = text;
                    answer.SubmissionDate = DateTime.Now;
                }
                else
                {
                    answer = new AssignmentAnswer
                    {
                        IdAssignment = _assignmentId,
                        IdStudent = App.IdUser,
                        AnswerText = text,
                        SubmissionDate = DateTime.Now
                    };
                    _context.AssignmentAnswers.Add(answer);
                    _context.SaveChanges(); // нужен Id для файлов
                }

                // Сохраняем файлы
                string userFolder = Path.Combine(RootFolder, $"Assignment_{_assignmentId}", $"User_{App.IdUser}");
                Directory.CreateDirectory(userFolder);

                foreach (var file in _attachedFiles)
                {
                    User user = App.Context.Users.FirstOrDefault(u => u.IdUser == App.IdUser);
                    string destFileName = $"{user.LastName}_{user.FirstName}_{Path.GetFileName(file)}";
                    string destPath = Path.Combine(userFolder, destFileName);

                    // Копируем файл
                    File.Copy(file, destPath, true);

                    // Сохраняем в БД
                    if (!_context.AssignmentFiles.Any(f => f.FilePath == destPath))
                    {
                        _context.AssignmentFiles.Add(new AssignmentFile
                        {
                            IdAssignmentAnswer = answer.IdAnswer,
                            OriginalFileName = Path.GetFileName(file),
                            StoredFileName = destFileName,
                            FilePath = destPath
                        });
                    }
                }

                _context.SaveChanges();

                MessageBox.Show("Ответ сохранён!");
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении ответа: {ex.Message}");
            }
        }
    }
}
