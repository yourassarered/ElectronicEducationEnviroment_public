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
    /// Логика взаимодействия для AssignGroupsWindow.xaml
    /// </summary>
    public partial class AssignGroupsWindow : Window
    {
        private readonly ElectronicEducationEnviromentEntities _context;
        private readonly Cours _course;

        public AssignGroupsWindow(Cours course)
        {
            InitializeComponent();
            _context = App.Context;
            _course = course;

            Title = $"Назначение групп на программу: {_course.CourseName}";
            LoadLists();
        }

        private void LoadLists()
        {
            var assignedGroupIds = _context.CourseParticipants
                .Where(cp => cp.IdCourse == _course.IdCourse)
                .Select(cp => cp.IdGroup)
                .ToList();

            AssignedGroupsList.ItemsSource = _context.Groups
                .Where(g => assignedGroupIds.Contains(g.IdGroup))
                .ToList();

            AvailableGroupsList.ItemsSource = _context.Groups
                .Where(g => !assignedGroupIds.Contains(g.IdGroup))
                .OrderBy(g => g.GroupCode)
                .ToList();
        }

        private void Assign_Click(object sender, RoutedEventArgs e)
        {
            var selectedGroups = AvailableGroupsList.SelectedItems.Cast<Group>().ToList();

            if (selectedGroups.Count == 0)
            {
                MessageBox.Show("Выберите хотя бы одну группу.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            foreach (var group in selectedGroups)
            {
                _context.CourseParticipants.Add(new CourseParticipant
                {
                    IdCourse = _course.IdCourse,
                    IdGroup = group.IdGroup
                });
            }

            _context.SaveChanges();
            LoadLists();
        }

        private void Unassign_Click(object sender, RoutedEventArgs e)
        {
            var selectedGroups = AssignedGroupsList.SelectedItems.Cast<Group>().ToList();

            if (selectedGroups.Count == 0)
            {
                MessageBox.Show("Выберите хотя бы одну группу для открепления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            foreach (var group in selectedGroups)
            {
                var link = _context.CourseParticipants
                    .FirstOrDefault(cp => cp.IdCourse == _course.IdCourse && cp.IdGroup == group.IdGroup);

                if (link != null)
                    _context.CourseParticipants.Remove(link);
            }

            _context.SaveChanges();
            LoadLists();
        }
    }
}

