using ElectronicEducationEnviroment.Admin;
using ElectronicEducationEnviroment.Teacher;
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

namespace ElectronicEducationEnviroment
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        private readonly ElectronicEducationEnviromentEntities _context = App.Context;
        public AuthWindow()
        {
            InitializeComponent();
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string login = TxtLogin.Text;
            string password = TxtPassword.Password;

                var user = _context.Users.FirstOrDefault(u => u.Login == login && u.Password == password);
                var role = user.Role.RoleName;
                if (user == null)
                {
                    MessageBox.Show("Неверный логин или пароль");
                    return;
                }

                switch (role)
                {
                    case "student":
                        App.IdUser = user.IdUser;
                        App.isTeacher = false;
                        StudentWindow studentWindow = new StudentWindow();
                        studentWindow.Show();
                        break;
                    case "teacher":
                      App.isTeacher = true;
                    TeacherWindow teacherWindow = new TeacherWindow();
                        teacherWindow.Show();
                        break;
                    case "admin":
                        AdminWindow adminWindow = new AdminWindow();
                        adminWindow.Show();
                        break;
                    default:
                        MessageBox.Show("Не удалось определить роль пользователя");
                        break;
                }

            this.Close();
        }

        private void AdminButton_Click(object sender, RoutedEventArgs e)
        {
            AdminWindow adminWindow = new AdminWindow();
            App.isTeacher = false;
            adminWindow.Show();
            this.Close();
        }

        private void TeacherButton_Click(object sender, RoutedEventArgs e)
        {
            App.isTeacher = true;
            App.IdUser = 8;
            TeacherWindow workerWindow = new TeacherWindow();
            workerWindow.Show();
            this.Close();
        }

        private void StudentButton_Click(object sender, RoutedEventArgs e)
        {
            User user = App.Context.Users.FirstOrDefault(u => u.Login == "student.student316");
            App.isTeacher = false;
            App.IdUser = user.IdUser;
            StudentWindow studentWindow = new StudentWindow();
            studentWindow.Show();
            this.Close();
        }
    }
}
