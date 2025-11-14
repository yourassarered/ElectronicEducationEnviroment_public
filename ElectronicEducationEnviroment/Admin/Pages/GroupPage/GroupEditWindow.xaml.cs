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
    /// Логика взаимодействия для GroupEditWindow.xaml
    /// </summary>
    public partial class GroupEditWindow : Window
    {
        private Group _group;

        public GroupEditWindow(Group group)
        {
            InitializeComponent();
            _group = group;
            LoadLists();
        }

        private void LoadLists()
        {
            CurrentList.ItemsSource = App.Context.GroupMembers
                .Where(m => m.IdGroup == _group.IdGroup)
                .Select(m => new
                {
                    m.IdStudent,
                    FullName = m.User.FirstName + " " + m.User.LastName
                })
                .ToList();

            AvailableList.ItemsSource = App.Context.Users
                .Where(u => u.IdRole == 1 && !u.GroupMembers.Any())
                .Select(u => new
                {
                    u.IdUser,
                    FullName = u.FirstName + " " + u.LastName
                })
                .ToList();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            foreach (dynamic item in AvailableList.SelectedItems)
            {
                var member = new GroupMember
                {
                    IdGroup = _group.IdGroup,
                    IdStudent = item.IdUser
                };
                App.Context.GroupMembers.Add(member);
            }
            App.Context.SaveChanges();
            LoadLists();
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            // Берём выбранных студентов
            var selectedItems = CurrentList.SelectedItems
                .Cast<object>()
                .Select(x =>
                {
                    // Достаём IdStudent через рефлексию, потому что это анонимный тип
                    var prop = x.GetType().GetProperty("IdStudent");
                    return (int)prop.GetValue(x);
                })
                .ToList();

            foreach (var studentId in selectedItems)
            {
                var member = App.Context.GroupMembers
                    .FirstOrDefault(m => m.IdGroup == _group.IdGroup && m.IdStudent == studentId);

                if (member != null)
                    App.Context.GroupMembers.Remove(member);
            }

            App.Context.SaveChanges();
            LoadLists();
        }


        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
