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
using System.Windows.Shapes;

namespace ElectronicEducationEnviroment.Admin.Pages.CourcePage
{
    /// <summary>
    /// Логика взаимодействия для AddEditProgramWindow.xaml
    /// </summary>
    public partial class AddEditProgramWindow : Window
    {
        private readonly ElectronicEducationEnviromentEntities _context;
        private readonly Cours _course;

        public AddEditProgramWindow(Cours course)
        {
            InitializeComponent();
            _context = App.Context;
            _course = course;

            if (_course != null)
            {
                Title = "Редактирование программы";
                NameBox.Text = _course.CourseName;
                DescBox.Text = _course.CourseDescription;
            }
            else
            {
                Title = "Добавление программы";
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show("Введите название программы.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_course == null)
            {
                var newCourse = new Cours
                {
                    CourseName = NameBox.Text.Trim(),
                    CourseDescription = DescBox.Text.Trim()
                };
                _context.Courses.Add(newCourse);
            }
            else
            {
                _course.CourseName = NameBox.Text.Trim();
                _course.CourseDescription = DescBox.Text.Trim();
            }

            _context.SaveChanges();
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
