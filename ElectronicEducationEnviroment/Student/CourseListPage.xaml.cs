using ElectronicEducationEnviroment.Admin;
using ElectronicEducationEnviroment.Teacher;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ElectronicEducationEnviroment.Student
{
    /// <summary>
    /// Логика взаимодействия для CourseListPage.xaml
    /// </summary>
    public partial class CourseListPage : Page
    {
        private readonly ElectronicEducationEnviromentEntities _context;

        public CourseListPage()
        {
            InitializeComponent();
            _context = App.Context;
            LoadCourses();
        }

        private void LoadCourses()
        {
            CoursesStack.Children.Clear();

            var coursesQuery = _context.Courses.AsQueryable();

            if (App.isTeacher)
            {
                var teacherCourseIds = _context.CourseTeachers
                    .Where(ct => ct.IdTeacher == App.IdUser)
                    .Select(ct => ct.IdCourse)
                    .ToList();

                coursesQuery = coursesQuery.Where(c => teacherCourseIds.Contains(c.IdCourse));
            }
            else
            {
                var studentGroup = _context.GroupMembers.FirstOrDefault(sg => sg.IdStudent == App.IdUser);
                if (studentGroup == null)
                {
                    MessageBox.Show("Вы не состоите ни в одной группе.");
                    return;
                }

                int groupId = studentGroup.IdGroup;

                var groupCourseIds = _context.CourseParticipants
                    .Where(cp => cp.IdGroup == groupId)
                    .Select(cp => cp.IdCourse)
                    .ToList();

                coursesQuery = coursesQuery.Where(c => groupCourseIds.Contains(c.IdCourse));
            }

            var courses = coursesQuery.OrderBy(c => c.IdCourse).ToList();

            if (courses.Count == 0)
            {
                var emptyMsg = new TextBlock
                {
                    Text = "Нет доступных курсов.",
                    FontSize = 16,
                    Foreground = Brushes.Gray,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 30, 0, 0)
                };
                CoursesStack.Children.Add(emptyMsg);
                return;
            }

            foreach (var course in courses)
            {
                var card = CreateCourseCard(course.IdCourse, course.CourseName, course.CourseDescription);
                CoursesStack.Children.Add(card);
            }
        }

        private Border CreateCourseCard(int id, string title, string description)
        {
            var colors = new Brush[]
            {
                Brushes.LightBlue, Brushes.LightCoral, Brushes.LightGreen,
                Brushes.LightGoldenrodYellow, Brushes.Plum, Brushes.LightPink
            };
            var rand = new Random(Guid.NewGuid().GetHashCode());
            var color = colors[rand.Next(colors.Length)];

            var border = new Border
            {
                Background = Brushes.White,
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Margin = new Thickness(0, 0, 0, 10),
                Padding = new Thickness(10),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var iconBox = new Border
            {
                Background = color,
                Width = 40,
                Height = 40,
                CornerRadius = new CornerRadius(4),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var icon = new TextBlock
            {
                Text = "📘",
                FontSize = 20,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            iconBox.Child = icon;

            Grid.SetColumn(iconBox, 0);
            grid.Children.Add(iconBox);

            var stack = new StackPanel
            {
                Orientation = Orientation.Vertical
            };

            var titleBlock = new TextBlock
            {
                Text = title,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(10, 0, 0, 5),
                TextWrapping = TextWrapping.Wrap
            };
            stack.Children.Add(titleBlock);

            if (!string.IsNullOrWhiteSpace(description))
            {
                var descBlock = new TextBlock
                {
                    Text = description,
                    Foreground = Brushes.Gray,
                    FontSize = 13,
                    Margin = new Thickness(10, 0, 0, 0),
                    TextWrapping = TextWrapping.Wrap
                };
                stack.Children.Add(descBlock);
            }

            Grid.SetColumn(stack, 1);
            grid.Children.Add(stack);

            border.Child = grid;

            border.MouseLeftButtonUp += (s, e) =>
            {
                NavigationService.Navigate(new CourseContentPage(id));
            };

            return border;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }


    }
}

