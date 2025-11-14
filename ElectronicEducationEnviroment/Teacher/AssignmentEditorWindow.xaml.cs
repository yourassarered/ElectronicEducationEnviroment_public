using ElectronicEducationEnviroment.Admin;
using System;
using System.Linq;
using System.Windows;

namespace ElectronicEducationEnviroment.Teacher
{
    public partial class AssignmentEditorWindow : Window
    {
        private readonly ElectronicEducationEnviromentEntities _context;
        private readonly int _courseId;
        private readonly Assignment _assignment;

        public AssignmentEditorWindow(int courseId, Assignment assignment = null)
        {
            InitializeComponent();
            _context = App.Context;
            _courseId = courseId;
            _assignment = assignment;

            LoadGroups();
            LoadAssignmentData();
        }

        private void LoadGroups()
        {
            GroupComboBox.ItemsSource = _context.Groups.ToList();
        }

        private void LoadAssignmentData()
        {
            if (_assignment != null)
            {
                TitleBox.Text = _assignment.AssignmentTitle;
                TextBoxDesc.Text = _assignment.AssignmentText;

                var group = _context.Groups.FirstOrDefault(g => g.IdGroup == _assignment.IdGroup);
                if (group != null)
                    GroupComboBox.SelectedItem = group;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleBox.Text))
            {
                MessageBox.Show("Введите название задания!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (GroupComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите группу!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedGroup = (Group)GroupComboBox.SelectedItem;

            if (_assignment == null)
            {
                var newTask = new Assignment
                {
                    IdCourse = _courseId,
                    IdGroup = selectedGroup.IdGroup,
                    AssignmentTitle = TitleBox.Text.Trim(),
                    AssignmentText = TextBoxDesc.Text.Trim(),
                    OpenDate = DateTime.Now,
                    DueDate = DateTime.Now.AddDays(7)
                };

                _context.Assignments.Add(newTask);
            }
            else
            {
                _assignment.AssignmentTitle = TitleBox.Text.Trim();
                _assignment.AssignmentText = TextBoxDesc.Text.Trim();
                _assignment.IdGroup = selectedGroup.IdGroup;
            }

            try
            {
                _context.SaveChanges();
                MessageBox.Show("Задание успешно сохранено!", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
