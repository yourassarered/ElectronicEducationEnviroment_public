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

namespace ElectronicEducationEnviroment.Admin.Pages.GroupPage
{
    /// <summary>
    /// Логика взаимодействия для GroupPage.xaml
    /// </summary>
    public partial class GroupPage : Page
    {
        public GroupPage()
        {
            InitializeComponent();
            LoadGroups();
        }

        private void LoadGroups()
        {
            var groups = App.Context.Groups
                .Select(g => new
                {
                    g.IdGroup,
                    g.GroupCode,
                    StudentCount = g.GroupMembers.Count()
                })
                .ToList();

            GroupsGrid.ItemsSource = groups;
        }

        private void AddGroup_Click(object sender, RoutedEventArgs e)
        {
            var window = new GroupAddWindow();
            if (window.ShowDialog() == true)
            {
                LoadGroups();
            }
        }

        private void EditGroup_Click(object sender, RoutedEventArgs e)
        {
            if (GroupsGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите группу для редактирования");
                return;
            }

            dynamic selected = GroupsGrid.SelectedItem;
            int id = selected.IdGroup;

            var group = App.Context.Groups.FirstOrDefault(g => g.IdGroup == id);

            if (group != null)
            {
                var window = new GroupEditWindow(group);
                if (window.ShowDialog() == true)
                    LoadGroups();
            }
        }

        private void DeleteGroup_Click(object sender, RoutedEventArgs e)
        {
            if (GroupsGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите группу для удаления");
                return;
            }

            dynamic selected = GroupsGrid.SelectedItem;
            int id = selected.IdGroup;

            var group = App.Context.Groups.FirstOrDefault(g => g.IdGroup == id);
            if (group != null)
            {
                if (MessageBox.Show($"Удалить группу {group.GroupCode}?", "Подтверждение", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    // Удаляем всех членов группы
                    var members = App.Context.GroupMembers.Where(m => m.IdGroup == group.IdGroup).ToList();
                    App.Context.GroupMembers.RemoveRange(members);

                    App.Context.Groups.Remove(group);
                    App.Context.SaveChanges();
                    LoadGroups();
                }
            }
        }

        private void Page_GotFocus(object sender, RoutedEventArgs e)
        {
            LoadGroups();
        }
    }
}
