using ElectronicEducationEnviroment.Admin;
using ElectronicEducationEnviroment.Student;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Border = System.Windows.Controls.Border;


namespace ElectronicEducationEnviroment.Teacher
{
    /// <summary>
    /// Логика взаимодействия для CourseContentPage.xaml
    /// </summary>
    public partial class CourseContentPage : Page
    {
        private readonly ElectronicEducationEnviromentEntities _context;
        private readonly int _courseId;
        private readonly Random _rand = new Random();

        private bool showLectureControls = App.isTeacher;
        private bool showTaskControls = App.isTeacher;

        public CourseContentPage(int courseId = 2)
        {
            InitializeComponent();

            _context = App.Context;
            _courseId = courseId;
            LoadContent();
            LoadGroups();

            if (App.isTeacher)
            {
               TeacherDock.Visibility = Visibility.Visible;
            }
            else
            {
                TeacherDock.Visibility = Visibility.Collapsed;
            }
        }

        private void EditLecture(int id)
        {
            var lecture = _context.Lectures.FirstOrDefault(l => l.IdLecture == id);
            if (lecture == null) return;

            var lectureEditorWindow = new LectureEditorWindow(_courseId, lecture);
            lectureEditorWindow.ShowDialog();
            LoadContent();
        }

        private void ManageLectureAccess(int id)
        {
            var lectureAccessWindow = new LectureAccessWindow(id);
            lectureAccessWindow.ShowDialog();
            LoadContent();
        }

        private void DeleteLecture(int id)
        {
            var lecture = _context.Lectures.FirstOrDefault(l => l.IdLecture == id);
            if (lecture == null) return;

            if (MessageBox.Show($"Удалить лекцию '{lecture.LectureTitle}'?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _context.Lectures.Remove(lecture);
                _context.SaveChanges();
                LoadContent();
            }
        }

        private void EditTask(int id)
        {
            Assignment assignment = App.Context.Assignments.FirstOrDefault(a => a.IdAssignment == id);
           AssignmentEditorWindow assignmentEditorWindow = new AssignmentEditorWindow(_courseId, assignment);
            assignmentEditorWindow.ShowDialog();
            LoadContent();
        }

        private void DeleteTask(int id)
        {
            var task = _context.Assignments.FirstOrDefault(t => t.IdAssignment == id);
        }

        private void ManageTaskAccess(int id)
        {
            AnswerCheckWindow answerCheckWindow = new AnswerCheckWindow(id); 
            answerCheckWindow.ShowDialog();
            LoadContent();
        }

        private void LoadContent()
        {
            ContentStack.Children.Clear();

            if (App.isTeacher)
            {
                LoadAllLecturesAndTasks();
                return;
            }

            // === Студент ===
            int studentId = App.IdUser;

            // Определяем группу студента
            var studentGroup = _context.GroupMembers.FirstOrDefault(g => g.IdStudent == studentId);
            if (studentGroup == null)
            {
                MessageBox.Show("Вы не состоите ни в одной группе. Доступ к материалам отсутствует.");
                return;
            }

            int groupId = studentGroup.IdGroup;

            // --- Лекции, доступные группе ---
            var accessibleLectureIds = _context.LectureAccesses
                .Where(a => a.IdGroup == groupId)
                .Select(a => a.IdLecture)
                .ToList();

            var lectures = _context.Lectures
                .Where(l => l.idCource == _courseId && accessibleLectureIds.Contains(l.IdLecture))
                .OrderBy(l => l.IdLecture)
                .ToList();

            foreach (var lecture in lectures)
            {
                var card = CreateCard(lecture.IdLecture, lecture.LectureTitle, true);
                ContentStack.Children.Add(card);
            }

            // --- Задания, доступные группе ---
            var tasks = _context.Assignments
                .Where(t => t.IdCourse == _courseId && t.IdGroup == groupId)
                .OrderBy(t => t.IdAssignment)
                .ToList();

            foreach (var task in tasks)
            {
                var card = CreateCard(task.IdAssignment, task.AssignmentTitle, false, task.OpenDate, task.DueDate);
                ContentStack.Children.Add(card);
            }
        }

        private void LoadAllLecturesAndTasks()
        {
            var lectures = _context.Lectures
                .Where(l => l.idCource == _courseId)
                .OrderBy(l => l.IdLecture)
                .ToList();

            foreach (var lecture in lectures)
            {
                var card = CreateCard(lecture.IdLecture, lecture.LectureTitle, true);
                ContentStack.Children.Add(card);
            }

            var tasks = _context.Assignments
                .Where(t => t.IdCourse == _courseId)
                .OrderBy(t => t.IdAssignment)
                .ToList();

            foreach (var task in tasks)
            {
                var card = CreateCard(task.IdAssignment, task.AssignmentTitle, false, task.OpenDate, task.DueDate);
                ContentStack.Children.Add(card);
            }
        }

        private Border CreateCard(int id, string title, bool isLecture, DateTime? openDate = null, DateTime? dueDate = null)
        {
            var colors = new Brush[]
            {
        Brushes.LightBlue, Brushes.LightCoral, Brushes.LightGreen,
        Brushes.LightGoldenrodYellow, Brushes.Plum, Brushes.LightPink,
        Brushes.LightCyan, Brushes.MistyRose, Brushes.LightSteelBlue, Brushes.PaleTurquoise
            };

            var color = colors[_rand.Next(colors.Length)];

            var border = new Border
            {
                Background = Brushes.White,
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Margin = new Thickness(0, 0, 0, 10),
                Padding = new Thickness(10),
                Cursor = Cursors.Hand
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var rectangle = new Border
            {
                Background = color,
                Width = 40,
                Height = 40,
                CornerRadius = new CornerRadius(4),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            var symbol = new TextBlock
            {
                Text = isLecture ? "📖" : "📝",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 18
            };
            rectangle.Child = symbol;

            Grid.SetColumn(rectangle, 0);
            grid.Children.Add(rectangle);

            var stack = new StackPanel { Orientation = Orientation.Vertical };

            var textBlock = new TextBlock
            {
                Text = title,
                FontSize = 16,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 5),
                TextWrapping = TextWrapping.Wrap
            };
            stack.Children.Add(textBlock);

            // === Раздел для преподавателя ===
            if (App.isTeacher && !isLecture)
            {
                int totalAnswers = _context.AssignmentAnswers.Count(a => a.IdAssignment == id);
                int uncheckedAnswers = _context.AssignmentAnswers.Count(a => a.IdAssignment == id && a.Grade == null);

                var statsText = new TextBlock
                {
                    Text = $"Ответов: {totalAnswers}, непроверенных: {uncheckedAnswers}",
                    Margin = new Thickness(10, 0, 0, 5),
                    FontSize = 13,
                    Foreground = uncheckedAnswers > 0 ? Brushes.OrangeRed : Brushes.Green,
                    FontWeight = FontWeights.SemiBold
                };
                stack.Children.Add(statsText);
            }
            // === Раздел для студента (уведомления о доступности и сроках) ===
            else if (!App.isTeacher && !isLecture)
            {
                var statusText = new TextBlock
                {
                    Margin = new Thickness(10, 0, 0, 5),
                    FontSize = 13,
                    FontWeight = FontWeights.SemiBold
                };

                var now = DateTime.Now;
                var studentId = App.IdUser;
                var existingAnswer = _context.AssignmentAnswers
                    .FirstOrDefault(a => a.IdAssignment == id && a.IdStudent == studentId);

                if (existingAnswer != null)
                {
                    statusText.Text = $"💾 Ответ сохранён ({existingAnswer.SubmissionDate:dd.MM.yyyy HH:mm})";
                    statusText.Foreground = Brushes.DodgerBlue;
                    border.Opacity = 1.0;
                }
                else if (openDate.HasValue && now < openDate.Value)
                {
                    statusText.Text = $"🕓 Ещё не началось (с {openDate.Value:dd.MM.yyyy})";
                    statusText.Foreground = Brushes.Gray;
                    border.Opacity = 0.6;
                }
                else if (dueDate.HasValue && now > dueDate.Value)
                {
                    statusText.Text = $"⏰ Просрочено (до {dueDate.Value:dd.MM.yyyy})";
                    statusText.Foreground = Brushes.Red;
                    border.Opacity = 0.7;
                }
                else
                {
                    statusText.Text = "✅ Доступно";
                    statusText.Foreground = Brushes.Green;
                    border.Opacity = 1.0;
                }

                stack.Children.Add(statusText);
            }

            // === Панель кнопок ===
            bool showControls = isLecture ? showLectureControls : showTaskControls;
            if (showControls)
            {
                var buttonPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(0, 5, 0, 0)
                };

                if (isLecture)
                {
                    var editBtn = new Button { Content = "Редактировать", Margin = new Thickness(5), Width = 100 };
                    editBtn.Click += (s, e) => EditLecture(id);

                    var accessBtn = new Button { Content = "Доступ", Margin = new Thickness(5), Width = 100 };
                    accessBtn.Click += (s, e) => ManageLectureAccess(id);

                    var deleteBtn = new Button { Content = "Удалить", Margin = new Thickness(5), Width = 100 };
                    deleteBtn.Click += (s, e) => DeleteLecture(id);

                    buttonPanel.Children.Add(editBtn);
                    buttonPanel.Children.Add(accessBtn);
                    buttonPanel.Children.Add(deleteBtn);
                }
                else
                {
                    var editBtn = new Button { Content = "Редактировать", Margin = new Thickness(5), Width = 100 };
                    editBtn.Click += (s, e) => EditTask(id);

                    var accessBtn = new Button { Content = "Проверить", Margin = new Thickness(5), Width = 100 };
                    accessBtn.Click += (s, e) => ManageTaskAccess(id);

                    var deleteBtn = new Button { Content = "Удалить", Margin = new Thickness(5), Width = 100 };
                    deleteBtn.Click += (s, e) => DeleteTask(id);

                    buttonPanel.Children.Add(editBtn);
                    buttonPanel.Children.Add(accessBtn);
                    buttonPanel.Children.Add(deleteBtn);
                }

                stack.Children.Add(buttonPanel);
            }

            Grid.SetColumn(stack, 1);
            grid.Children.Add(stack);
            border.Child = grid;

            border.MouseLeftButtonUp += (s, e) =>
            {
                if (showControls)
                    return;

                if (isLecture)
                    NavigationService.Navigate(new LectureViewPage(id));
                else
                    NavigationService.Navigate(new TaskViewPage(id));
            };

            return border;
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            var lectureWindow = new LectureEditorWindow(_courseId);
            lectureWindow.ShowDialog();
            LoadContent();
        }

        private void AddTaskBtn_Click(object sender, RoutedEventArgs e)
        {
            AssignmentEditorWindow assignmentEditorWindow = new AssignmentEditorWindow(_courseId);
            assignmentEditorWindow.ShowDialog();
            LoadContent();
        }
        private void LoadGroups()
        {
            var groups = App.Context.Groups.OrderBy(g => g.GroupCode).ToList();
            GroupsComboBox.ItemsSource = groups;
            GroupsComboBox.DisplayMemberPath = "GroupCode";
            GroupsComboBox.SelectedValuePath = "IdGroup";
        }


        private void ExportGrades_Click(object sender, RoutedEventArgs e)
        {
            if (GroupsComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите группу для экспорта.");
                return;
            }

            int groupId = (int)GroupsComboBox.SelectedValue;

            var students = App.Context.GroupMembers
                .Where(g => g.IdGroup == groupId)
                .OrderBy(s => s.idGroupMember)
                .ToList();

            var assignments = App.Context.Assignments
                .Where(a => a.IdCourse == _courseId && a.IdGroup == groupId)
                .OrderBy(a => a.IdAssignment)
                .ToList();

            if (!students.Any() || !assignments.Any())
            {
                MessageBox.Show("Нет данных для экспорта.");
                return;
            }

            ExportGradesToExcel(students, assignments);
        }

        private void ExportGradesToExcel(List<GroupMember> students, List<Assignment> assignments)
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel Files|*.xlsx",
                    FileName = $"Grades_{DateTime.Now:yyyyMMdd}.xlsx"
                };

                if (saveFileDialog.ShowDialog() != true)
                    return;

                ExcelPackage.License.SetNonCommercialPersonal("Иван");

                using (var package = new ExcelPackage())
                {
                    var sheet = package.Workbook.Worksheets.Add("Оценки");

                    // Заголовки
                    sheet.Cells[1, 1].Value = "ФИО студента";
                    int col = 2;

                    foreach (var a in assignments)
                    {
                        sheet.Cells[1, col].Value = a.AssignmentTitle ?? "Без названия";
                        col++;
                    }

                    sheet.Cells[1, col].Value = "Средний балл";

                    int row = 2;

                    foreach (var student in students)
                    {
                        // Защита от null
                        string firstName = student?.User?.FirstName ?? "(Не указано)";
                        string lastName = student?.User?.LastName ?? "";
                        sheet.Cells[row, 1].Value = $"{firstName} {lastName}".Trim();

                        col = 2;
                        double total = 0;
                        int gradedCount = 0;

                        foreach (var task in assignments)
                        {
                            // Исправлено: IdStudent должен сравниваться с IdUser, а не с idGroupMember
                            var answer = App.Context.AssignmentAnswers
                                .FirstOrDefault(ans =>
                                    ans.IdAssignment == task.IdAssignment &&
                                    ans.IdStudent == student.IdStudent);

                            string grade = answer?.Grade ?? "";
                            sheet.Cells[row, col].Value = grade;

                            if (double.TryParse(grade, out double gradeValue))
                            {
                                total += gradeValue;
                                gradedCount++;
                            }
                            col++;
                        }

                        double avg = gradedCount > 0 ? Math.Round(total / gradedCount, 2) : 0;
                        sheet.Cells[row, col].Value = gradedCount > 0 ? avg.ToString("0.00") : "-";
                        row++;
                    }

                    // Красивое форматирование
                    using (var range = sheet.Cells[1, 1, row - 1, col])
                    {
                        range.AutoFitColumns();
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    }

                    // Сохраняем файл
                    File.WriteAllBytes(saveFileDialog.FileName, package.GetAsByteArray());

                    MessageBox.Show("Экспорт выполнен успешно!", "Готово",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте в Excel:\n{ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
