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
    /// Логика взаимодействия для AddEditUserWindow.xaml
    /// </summary>
    public partial class AddEditUserWindow : Window
    {
        private readonly ElectronicEducationEnviromentEntities _context;
        private readonly User _user;
        public AddEditUserWindow(ElectronicEducationEnviromentEntities context, User user)
        {
            InitializeComponent();
            _context = context;
            _user = user;

            
            RoleComboBox.ItemsSource = _context.Roles.ToList();

            if (_user != null)
            {
                LastNameTextBox.Text = _user.LastName;
                FirstNameTextBox.Text = _user.FirstName;
                EmailTextBox.Text = _user.Email;
                LoginTextBox.Text = _user.Login;
                PasswordTextBox.Text = _user.Password;
                RoleComboBox.SelectedValue = _user.IdRole;
            }
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (_user != null)
            {
                _user.LastName = LastNameTextBox.Text;
                _user.FirstName = FirstNameTextBox.Text;
                _user.Email = EmailTextBox.Text;
                _user.Login = LoginTextBox.Text;
                _user.Password = PasswordTextBox.Text;
                _user.IdRole = (int)RoleComboBox.SelectedValue;

                if (_user.IdRole == 0)
                    _context.Users.Add(_user); 

                _context.SaveChanges();
                DialogResult = true;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}

