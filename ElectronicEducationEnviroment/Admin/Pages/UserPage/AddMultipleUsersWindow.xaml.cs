using Microsoft.Win32;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.VisualBasic;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;

namespace ElectronicEducationEnviroment.Admin
{
    public partial class AddMultipleUsersWindow : Window
    {
        private readonly ElectronicEducationEnviromentEntities _context;
        private readonly List<User> _newUsers = new List<User>();

        public AddMultipleUsersWindow(ElectronicEducationEnviromentEntities context)
        {
            InitializeComponent();
            _context = context;

            try
            {
                RoleComboBox.ItemsSource = _context.Roles.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки ролей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddToList_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(LastNameTextBox.Text) ||
                    string.IsNullOrWhiteSpace(FirstNameTextBox.Text) ||
                    RoleComboBox.SelectedValue == null)
                {
                    MessageBox.Show("Заполните все обязательные поля!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string login = GenerateLogin(LastNameTextBox.Text, FirstNameTextBox.Text);
                string password = GeneratePassword();

                var newUser = new User
                {
                    LastName = LastNameTextBox.Text.Trim(),
                    FirstName = FirstNameTextBox.Text.Trim(),
                    Email = EmailTextBox.Text.Trim(),
                    Login = login,
                    Password = password,
                    IdRole = (int)RoleComboBox.SelectedValue
                };

                _newUsers.Add(newUser);

                AddedUsersListBox.Items.Add(new
                {
                    DisplayInfo = $"{newUser.LastName} {newUser.FirstName} | {newUser.Login} | {newUser.Password}"
                });

                ClearFields_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении пользователя в список: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearFields_Click(object sender, RoutedEventArgs e)
        {
            LastNameTextBox.Clear();
            FirstNameTextBox.Clear();
            EmailTextBox.Clear();
            RoleComboBox.SelectedIndex = -1;
        }

        private void SaveAll_Click(object sender, RoutedEventArgs e)
        {
            if (!_newUsers.Any())
            {
                MessageBox.Show("Нет пользователей для сохранения!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _context.Users.AddRange(_newUsers);
                _context.SaveChanges();

                MessageBox.Show($"Добавлено {_newUsers.Count} пользователей!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                AskExportToExcel();
                Close();
            }
            catch (DbUpdateException dbEx)
            {
                HandleDatabaseError(dbEx);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении пользователей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveAndCreateGroup_Click(object sender, RoutedEventArgs e)
        {
            if (!_newUsers.Any())
            {
                MessageBox.Show("Сначала добавьте студентов в список!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Сохраняем пользователей
                _context.Users.AddRange(_newUsers);
                _context.SaveChanges();

                // Создание группы
                string groupCode = Interaction.InputBox("Введите код новой группы:", "Создание группы", "GROUP-1");
                if (string.IsNullOrWhiteSpace(groupCode))
                {
                    MessageBox.Show("Группа не создана: код не введён.", "Отмена", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var newGroup = new Group { GroupCode = groupCode.Trim() };
                _context.Groups.Add(newGroup);
                _context.SaveChanges();

                // Добавляем студентов
                foreach (var student in _newUsers)
                {
                    if (student.IdRole == 1) // студент
                    {
                        _context.GroupMembers.Add(new GroupMember
                        {
                            IdGroup = newGroup.IdGroup,
                            IdStudent = student.IdUser
                        });
                    }
                }

                _context.SaveChanges();

                MessageBox.Show($"Создана группа {newGroup.GroupCode} и добавлено {_newUsers.Count(u => u.IdRole == 1)} студентов!",
                    "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

                AskExportToExcel();
                Close();
            }
            catch (DbUpdateException dbEx)
            {
                HandleDatabaseError(dbEx);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении группы: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void HandleDatabaseError(DbUpdateException dbEx)
        {
            var sqlEx = dbEx.GetBaseException() as SqlException;
            if (sqlEx != null)
            {
                if (sqlEx.Number == 2627 || sqlEx.Number == 2601) // нарушение уникального ключа
                {
                    MessageBox.Show("Ошибка: обнаружен дублирующийся логин или email.\nПроверьте, что логины уникальны.",
                        "Ошибка БД", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show($"Ошибка базы данных (код {sqlEx.Number}): {sqlEx.Message}",
                        "Ошибка SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show($"Ошибка при обновлении базы данных: {dbEx.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AskExportToExcel()
        {
            try
            {
                if (MessageBox.Show("Экспортировать данные в Excel?", "Экспорт", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    ExportToExcel();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при экспорте: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportToExcel()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = "NewUsers.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    ExcelPackage.License.SetNonCommercialPersonal("Иван");
                    using (var package = new ExcelPackage())
                    {
                        var ws = package.Workbook.Worksheets.Add("Пользователи");
                        ws.Cells[1, 1].Value = "ФИО";
                        ws.Cells[1, 2].Value = "Логин";
                        ws.Cells[1, 3].Value = "Пароль";

                        for (int i = 0; i < _newUsers.Count; i++)
                        {
                            var u = _newUsers[i];
                            ws.Cells[i + 2, 1].Value = $"{u.LastName} {u.FirstName}";
                            ws.Cells[i + 2, 2].Value = u.Login;
                            ws.Cells[i + 2, 3].Value = u.Password;
                        }

                        package.SaveAs(new System.IO.FileInfo(saveFileDialog.FileName));
                    }

                    MessageBox.Show("Экспорт выполнен успешно!", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при экспорте в Excel: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private string GenerateLogin(string lastName, string firstName)
        {
            try
            {
                string translitLast = Transliterate(lastName);
                string translitFirst = Transliterate(firstName);
                return $"{translitLast.ToLower()}.{translitFirst.ToLower()}{new Random().Next(100, 999)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при генерации логина: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return $"user{DateTime.Now.Ticks}";
            }
        }

        private string Transliterate(string text)
        {
            Dictionary<char, string> translitMap = new Dictionary<char, string>
            {
                {'а',"a"}, {'б',"b"}, {'в',"v"}, {'г',"g"}, {'д',"d"}, {'е',"e"},
                {'ё',"e"}, {'ж',"zh"}, {'з',"z"}, {'и',"i"}, {'й',"y"}, {'к',"k"},
                {'л',"l"}, {'м',"m"}, {'н',"n"}, {'о',"o"}, {'п',"p"}, {'р',"r"},
                {'с',"s"}, {'т',"t"}, {'у',"u"}, {'ф',"f"}, {'х',"kh"}, {'ц',"ts"},
                {'ч',"ch"}, {'ш',"sh"}, {'щ',"shch"}, {'ы',"y"}, {'э',"e"}, {'ю',"yu"},
                {'я',"ya"},
                {'А',"A"}, {'Б',"B"}, {'В',"V"}, {'Г',"G"}, {'Д',"D"}, {'Е',"E"},
                {'Ё',"E"}, {'Ж',"Zh"}, {'З',"Z"}, {'И',"I"}, {'Й',"Y"}, {'К',"K"},
                {'Л',"L"}, {'М',"M"}, {'Н',"N"}, {'О',"O"}, {'П',"P"}, {'Р',"R"},
                {'С',"S"}, {'Т',"T"}, {'У',"U"}, {'Ф',"F"}, {'Х',"Kh"}, {'Ц',"Ts"},
                {'Ч',"Ch"}, {'Ш',"Sh"}, {'Щ',"Shch"}, {'Ы',"Y"}, {'Э',"E"}, {'Ю',"Yu"},
                {'Я',"Ya"}
            };

            var sb = new StringBuilder();
            foreach (var c in text)
                sb.Append(translitMap.ContainsKey(c) ? translitMap[c] : c.ToString());
            return sb.ToString();
        }

        private string GeneratePassword(int length = 8)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var rnd = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[rnd.Next(s.Length)]).ToArray());
        }
    }
}
