using SchoolApp.Models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WaterDelivery.Data;

namespace WaterDelivery.Views.Windows
{
    public partial class LoginWindow : Window
    {
        private string _password;

        public LoginWindow()
        {
            InitializeComponent();
            Loaded += (s, e) => loginTextBox.Focus();
        }

        private void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            string login = loginTextBox.Text;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(_password))
            {
                errorMessageTextBlock.Text = "Пожалуйста, заполните все поля.";
                errorMessageTextBlock.Visibility = Visibility.Visible;
                return;
            }

            try
            {
                using (var context = new WaterDeliveryEntities())
                {
                    var user = context.User.FirstOrDefault(u => u.UserName == login && u.Password == _password);

                    if (user != null)
                    {
                        CurrentUser.UserId = user.UserId;
                        CurrentUser.UserName = user.UserName;
                        CurrentUser.Email = user.Email;
                        CurrentUser.Phone = user.Phone;
                        CurrentUser.UserRoleId = user.UserRoleId;
                        CurrentUser.LastLoginDate = DateTime.Now;

                        LogLoginTime(user.UserId, context);

                        var employee = context.Employee.FirstOrDefault(w => w.UserId == user.UserId);
                        if (employee != null)
                        {
                            CurrentUser.EmployeeId = employee.EmployeeId;
                            CurrentUser.FirstName = employee.FirstName;
                            CurrentUser.MiddleName = employee.MiddleName;
                            CurrentUser.LastName = employee.LastName;
                            CurrentUser.PositionId = employee.PositionId;
                        }

                        MainWindow mainWindow = new MainWindow();
                        mainWindow.Show();
                        this.Close();
                    }
                    else
                    {
                        errorMessageTextBlock.Visibility = Visibility.Visible;
                        errorMessageTextBlock.Text = "Неверный логин или пароль";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при входе в систему: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LogLoginTime(int userId, WaterDeliveryEntities context)
        {
            try
            {
                var user = context.User.Find(userId);
                if (user != null)
                {
                    user.LastLoginDate = DateTime.Now;
                    context.SaveChanges();
                }
            }
            catch (Exception)
            {
            }
        }

        private void password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _password = ((PasswordBox)sender).Password;
        }

        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void btn_maximize_Click(object sender, RoutedEventArgs e)
        {
            SwitchWindowState();
        }

        private void btn_minimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void DockPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                SwitchWindowState();
                return;
            }

            if (WindowState == WindowState.Maximized)
            {
                return;
            }
            else
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                    DragMove();
            }
        }

        private void MaximizeWindow()
        {
            WindowState = WindowState.Maximized;
        }

        private void RestoreWindow()
        {
            WindowState = WindowState.Normal;
        }

        private void SwitchWindowState()
        {
            if (WindowState == WindowState.Normal)
                MaximizeWindow();
            else
                RestoreWindow();
        }
    }
}