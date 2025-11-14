using ElectronicEducationEnviroment.Admin.Pages;
using ElectronicEducationEnviroment.Admin.Pages.CourcePage;
using ElectronicEducationEnviroment.Admin.Pages.GroupPage;
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

namespace ElectronicEducationEnviroment.Admin
{
    /// <summary>
    /// Логика взаимодействия для AdminWindow.xaml
    /// </summary>
    public partial class AdminWindow : Window
    {
        public AdminWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new UsersPage());

        }
        private void BtnUsers_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new UsersPage());
        }

        private void BtnCourses_Click(object sender, RoutedEventArgs e)
        {
             MainFrame.Navigate(new AssignProgramsPage());
        }

       

        private void BtnGroups_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new GroupPage());
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            AuthWindow authWindow = new AuthWindow();
            authWindow.Show();
            this.Close();
        }
    }
}
