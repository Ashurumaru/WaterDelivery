using SchoolApp.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WaterDelivery.Data;
using WaterDelivery.Views.Pages;

namespace WaterDelivery.Views.Windows
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SetUserInfo();

            // Переход на панель управления при запуске
            NavigateToPage("DashboardPage");
            HighlightActiveButton(btnDashboard);
        }

        /// <summary>
        /// Заполняет информацию о текущем пользователе
        /// </summary>
        private void SetUserInfo()
        {
            if (!string.IsNullOrEmpty(CurrentUser.FirstName) && !string.IsNullOrEmpty(CurrentUser.LastName))
            {
                userInitials.Text = $"{CurrentUser.FirstName[0]}{CurrentUser.LastName[0]}";
                string fullName = $"{CurrentUser.LastName} {CurrentUser.FirstName[0]}";
                if (!string.IsNullOrEmpty(CurrentUser.MiddleName))
                    fullName += $" {CurrentUser.MiddleName[0]}";

                userName.Text = fullName;
            }
            else if (!string.IsNullOrEmpty(CurrentUser.UserName) && CurrentUser.UserName.Length > 0)
            {
                userInitials.Text = CurrentUser.UserName[0].ToString().ToUpper();
                userName.Text = CurrentUser.UserName;
            }
            else
            {
                userInitials.Text = "?";
                userName.Text = "Неизвестный пользователь";
            }

            // Установка роли пользователя
            userRole.Text = GetUserRoleName(CurrentUser.UserRoleId);
        }

        /// <summary>
        /// Возвращает название роли пользователя
        /// </summary>
        private string GetUserRoleName(int userRoleId)
        {
            switch (userRoleId)
            {
                case 1:
                    return "Администратор";
                case 2:
                    return "Диспетчер";
                case 3:
                    return "Кладовщик";
                case 4:
                    return "Водитель";
                case 5:
                    return "Бухгалтер";
                case 6:
                    return "Клиент";
                default:
                    return "Пользователь";
            }
        }

        /// <summary>
        /// Обработчик нажатия на кнопки навигации
        /// </summary>
        private void NavigateButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                // Сбрасываем стили всех кнопок меню
                ResetMenuButtonsStyle();

                // Устанавливаем активный стиль для нажатой кнопки
                HighlightActiveButton(button);

                string pageName = button.Tag as string;

                // Переходим на выбранную страницу
                NavigateToPage(pageName);
            }
        }

        /// <summary>
        /// Устанавливает активный стиль для кнопки
        /// </summary>
        private void HighlightActiveButton(Button button)
        {
            button.Style = (Style)FindResource("ActiveTabMenuButtonStyle");
            button.Background = (SolidColorBrush)FindResource("PrimaryLightColor");
            button.Foreground = Brushes.White;
        }

        /// <summary>
        /// Сбрасывает стили всех кнопок меню
        /// </summary>
        private void ResetMenuButtonsStyle()
        {
            // Сбрасываем стили основных кнопок
            var mainButtons = new[]
            {
                btnDashboard, btnOrders, btnClients, btnVehicles,
                btnWarehouse, btnProducts, btnEmployees,
                btnHandbook, btnReports
            };

            foreach (var btn in mainButtons)
            {
                btn.Style = (Style)FindResource("TabMenuButtonStyle");
                btn.Background = Brushes.Transparent;
                btn.Foreground = Brushes.White;
            }
        }

        /// <summary>
        /// Создает и переходит на выбранную страницу
        /// </summary>
        private void NavigateToPage(string pageName)
        {
            Page page = null;

            switch (pageName)
            {
                case "DashboardPage":
                    page = new DashboardPage();
                    break;
                case "OrdersPage":
                    // page = new OrdersPage();
                    break;
                case "ClientsPage":
                    page = new ClientsPage();
                    break;
                case "VehiclesPage":
                    page = new VehiclesPage();
                    break;
                case "WarehousePage":
                    // page = new WarehousePage();
                    break;
                case "ProductsPage":
                    page = new ProductsPage();
                    break;
                case "EmployeesPage":
                    page = new EmployeesPage();
                    break;
                case "HandbookPage":
                    // page = new HandbookPage();
                    break;
                case "ReportsPage":
                    // page = new ReportsPage();
                    break;
                default:
                    page = new DashboardPage();
                    break;
            }

            if (page != null)
            {
                MainFrame.Navigate(page);

                // Обновляем заголовок страницы из её свойства Title
                if (page.Title != null)
                {
                    pageTitle.Text = page.Title;
                }
            }
        }

        /// <summary>
        /// Обработчик кнопки выхода из системы
        /// </summary>
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите выйти из системы?",
                "Подтверждение выхода", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                ClearCurrentUserData();

                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();

                this.Close();
            }
        }

        /// <summary>
        /// Очищает данные текущего пользователя
        /// </summary>
        private void ClearCurrentUserData()
        {
            CurrentUser.UserId = 0;
            CurrentUser.UserName = null;
            CurrentUser.Email = null;
            CurrentUser.Phone = null;
            CurrentUser.UserRoleId = 0;
            CurrentUser.LastLoginDate = null;
            CurrentUser.EmployeeId = null;
            CurrentUser.FirstName = null;
            CurrentUser.MiddleName = null;
            CurrentUser.LastName = null;
            CurrentUser.PositionId = null;
        }

        /// <summary>
        /// Обработчик кнопки сворачивания окна
        /// </summary>
        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Обработчик кнопки разворачивания/восстановления окна
        /// </summary>
        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
        {
            SwitchWindowState();
        }

        /// <summary>
        /// Обработчик кнопки закрытия окна
        /// </summary>
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Обработчик двойного клика или перетаскивания по заголовку окна
        /// </summary>
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

        /// <summary>
        /// Переключает состояние окна между развернутым и нормальным
        /// </summary>
        private void SwitchWindowState()
        {
            if (WindowState == WindowState.Normal)
                WindowState = WindowState.Maximized;
            else
                WindowState = WindowState.Normal;
        }
    }
}