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

namespace ElectronicEducationEnviroment.Admin.Pages.GroupPage
{
    /// <summary>
    /// Логика взаимодействия для GroupAddWindow.xaml
    /// </summary>
    public partial class GroupAddWindow : Window
    {
        public GroupAddWindow()
        {
            InitializeComponent();
            LoadStudents();
        }

        private void LoadStudents()
        {
            var students = App.Context.Users
                .Where(u => u.IdRole == 1 && !u.GroupMembers.Any())
                .Select(u => new
                {
                    u.IdUser,
                    FullName = u.FirstName + " " + u.LastName
                })
                .ToList();

            StudentsList.ItemsSource = students;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(GroupCodeBox.Text))
            {
                MessageBox.Show("Введите код группы!");
                return;
            }

            var group = new Group { GroupCode = GroupCodeBox.Text };
            App.Context.Groups.Add(group);
            App.Context.SaveChanges();

            foreach (dynamic item in StudentsList.SelectedItems)
            {
                var member = new GroupMember
                {
                    IdGroup = group.IdGroup,
                    IdStudent = item.IdUser
                };
                App.Context.GroupMembers.Add(member);
            }

            App.Context.SaveChanges();
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
