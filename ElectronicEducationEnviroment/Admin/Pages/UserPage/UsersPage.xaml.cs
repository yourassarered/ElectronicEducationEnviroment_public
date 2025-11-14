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

namespace ElectronicEducationEnviroment.Admin.Pages
{
    /// <summary>
    /// Логика взаимодействия для UsersPage.xaml
    /// </summary>
    public partial class UsersPage : Page
    {
        public static ElectronicEducationEnviromentEntities _context = new ElectronicEducationEnviromentEntities();
        private int _currentPage = 1;
        private int _pageSize = 10;
        private int _totalPages = 1;

        public UsersPage()
        {
            InitializeComponent();
            
            LoadPageSize();
            LoadUsers();
        }
        private void LoadPageSize()
        {
            if (PageSizeComboBox.SelectedItem is ComboBoxItem item)
            {
                _pageSize = int.Parse(item.Content.ToString());
            }
        }
        private void LoadUsers()
        {
            if (_context == null)
            {
                MessageBox.Show("_context равен null!");
                return;
            }

            try
            {
                int totalUsers = _context.Users.Count();
                _totalPages = (int)Math.Ceiling(totalUsers / (double)_pageSize);

                var users = _context.Users
                                    .OrderBy(u => u.IdUser)
                                    .Take(_pageSize)
                                    .ToList();

                UsersDataGrid.ItemsSource = users;
                PageInfoText.Text = $"Страница {_currentPage} из {_totalPages}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке пользователей: {ex.Message}");
            }
        }



        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            var newUser = new User();
            var window = new AddEditUserWindow(_context, newUser) { Owner = Window.GetWindow(this) };
            if (window.ShowDialog() == true)
            {
                LoadUsers();
            }
        }

        // Редактирование
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (UsersDataGrid.SelectedItem is User user)
            {
                var window = new AddEditUserWindow(_context, user) { Owner = Window.GetWindow(this) };
                if (window.ShowDialog() == true)
                {
                    LoadUsers();
                }
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!(UsersDataGrid.SelectedItem is User selectedUser))
            {
                MessageBox.Show("Выберите пользователя для удаления.", "Удаление", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Загружаем актуальный объект из контекста
            var user = _context.Users.FirstOrDefault(u => u.IdUser == selectedUser.IdUser);
            if (user == null)
            {
                MessageBox.Show("Пользователь не найден в базе данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Загружаем все связанные сущности
            var memberships = _context.GroupMembers.Where(gm => gm.IdStudent == user.IdUser).ToList();
            var answers = _context.AssignmentAnswers.Where(a => a.IdStudent == user.IdUser).ToList();
            var answerFiles = _context.AssignmentFiles.Where(f => answers.Select(a => a.IdAnswer).Contains(f.IdAssignmentAnswer)).ToList();
            var teacherLinks = _context.CourseTeachers.Where(ct => ct.IdTeacher == user.IdUser).ToList();

            // Показываем предупреждение
            string groupNames = memberships.Any()
                ? string.Join(", ", memberships.Select(m =>
                    m.Group != null
                        ? m.Group.GroupCode
                        : _context.Groups.FirstOrDefault(g => g.IdGroup == m.IdGroup)?.GroupCode ?? $"(Id={m.IdGroup})"))
                : "нет";

            var confirmResult = MessageBox.Show(
                $"Удалить пользователя {user.LastName} {user.FirstName}?\n\n" +
                $"Группы: {groupNames}\n" +
                $"Ответов: {answers.Count}, файлов ответов: {answerFiles.Count}, курсов (как преподаватель): {teacherLinks.Count}\n\n" +
                $"Все связанные данные будут удалены безвозвратно. Продолжить?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmResult != MessageBoxResult.Yes)
                return;

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // 1. Удаляем файлы ответов (если есть физические файлы на диске — удаляем и их)
                    if (answerFiles.Any())
                    {
                        _context.AssignmentFiles.RemoveRange(answerFiles);
                    }

                    // 2. Удаляем ответы
                    if (answers.Any())
                        _context.AssignmentAnswers.RemoveRange(answers);

                    // 3. Удаляем членства в группах
                    if (memberships.Any())
                        _context.GroupMembers.RemoveRange(memberships);

                    // 4. Удаляем привязки преподавателя
                    if (teacherLinks.Any())
                        _context.CourseTeachers.RemoveRange(teacherLinks);

                

                    // 6. Удаляем пользователя
                    _context.Users.Remove(user);
                    _context.SaveChanges();

                    transaction.Commit();
                    MessageBox.Show("Пользователь и все связанные данные успешно удалены.", "Удаление", MessageBoxButton.OK, MessageBoxImage.Information);

                    LoadUsers();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    string err = ex.GetBaseException()?.Message ?? ex.Message;
                    MessageBox.Show($"Ошибка при удалении пользователя: {err}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }



        private void BtnPrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                LoadUsers();
            }
        }

        private void BtnNextPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                LoadUsers();
            }
        }

        private void PageSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PageSizeComboBox.SelectedItem is ComboBoxItem item)
            {
                _pageSize = int.Parse(item.Content.ToString());
                _currentPage = 1;
                LoadUsers();
            }
        }

        private void MultipleAddButton_Click(object sender, RoutedEventArgs e)
        {
            AddMultipleUsersWindow addMultipleUsersWindow = new AddMultipleUsersWindow(_context);
            addMultipleUsersWindow.ShowDialog();
            LoadUsers();

        }
    }
}
