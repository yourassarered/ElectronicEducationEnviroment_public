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

namespace ElectronicEducationEnviroment.Admin.Pages.CourcePage
{
    /// <summary>
    /// Логика взаимодействия для AssignProgramsPage.xaml
    /// </summary>
    public partial class AssignProgramsPage : Page
    {
        private readonly ElectronicEducationEnviromentEntities _context;

        public AssignProgramsPage()
        {
            InitializeComponent();
            _context = App.Context;

            LoadPrograms();
        }

        private void LoadPrograms()
        {
            ProgramsGrid.ItemsSource = _context.Courses.ToList();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddEditProgramWindow(null);
            if (window.ShowDialog() == true)
                LoadPrograms();
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            var selected = ProgramsGrid.SelectedItem as Cours;
            if (selected == null)
            {
                MessageBox.Show("Выберите учебную программу.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var window = new AddEditProgramWindow(selected);
            if (window.ShowDialog() == true)
                LoadPrograms();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var selected = ProgramsGrid.SelectedItem as Cours;
            if (selected == null)
            {
                MessageBox.Show("Выберите учебную программу для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Удалить программу '{selected.CourseName}' и все связанные данные?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                var assignments = _context.Assignments.Where(a => a.IdCourse == selected.IdCourse).ToList();
                foreach (var assignment in assignments)
                {
                    _context.AssignmentAnswers.RemoveRange(_context.AssignmentAnswers.Where(ans => ans.IdAssignment == assignment.IdAssignment));
                }
                _context.Assignments.RemoveRange(assignments);

                var lectures = _context.Lectures.Where(l => l.idCource == selected.IdCourse).ToList();
                foreach (var lecture in lectures)
                {
                    _context.LectureAccesses.RemoveRange(_context.LectureAccesses.Where(la => la.IdLecture == lecture.IdLecture));
                }

                _context.Lectures.RemoveRange(lectures);

                var participants = _context.CourseParticipants.Where(cp => cp.IdCourse == selected.IdCourse).ToList();
                _context.CourseParticipants.RemoveRange(participants);

                var teachers = _context.CourseTeachers.Where(ct => ct.IdCourse == selected.IdCourse).ToList();
                _context.CourseTeachers.RemoveRange(teachers);

                _context.Courses.Remove(selected);

                _context.SaveChanges();

                MessageBox.Show($"Курс '{selected.CourseName}' и все связанные данные удалены.", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);

                LoadPrograms();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении курса: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AssignGroups_Click(object sender, RoutedEventArgs e)
        {
            var selected = ProgramsGrid.SelectedItem as Cours;
            if (selected == null)
            {
                MessageBox.Show("Выберите учебную программу.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            new AssignGroupsWindow(selected).ShowDialog();
        }

        private void AssignTeachers_Click(object sender, RoutedEventArgs e)
        {
            var selected = ProgramsGrid.SelectedItem as Cours;
            if (selected == null)
            {
                MessageBox.Show("Выберите учебную программу.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            new AssignTeachersWindow(selected).ShowDialog();
        }
    }
}
