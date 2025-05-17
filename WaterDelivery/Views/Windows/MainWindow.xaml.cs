using SchoolApp.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WaterDelivery.Views.Windows
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SetUserInitials();
            NavigateToPage("OrdersPage");
        }

        private void SetUserInitials()
        {
            if (!string.IsNullOrEmpty(CurrentUser.FirstName) && !string.IsNullOrEmpty(CurrentUser.LastName))
            {
                userInitials.Text = $"{CurrentUser.FirstName[0]}{CurrentUser.LastName[0]}";
                string fullName = $"{CurrentUser.LastName} {CurrentUser.FirstName}";
                if (!string.IsNullOrEmpty(CurrentUser.MiddleName))
                    fullName += $" {CurrentUser.MiddleName}";

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

            userRole.Text = CurrentUser.IsAdmin ? "Администратор" : "Диспетчер";
        }

        private void NavigateButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                ResetMenuButtonsStyle();

                button.Style = (Style)FindResource("ActiveTabMenuButtonStyle");

                string pageName = button.Tag as string;

                UpdatePageTitle(pageName);

                NavigateToPage(pageName);
            }
        }

        private void ResetMenuButtonsStyle()
        {
            btnOrders.Style = (Style)FindResource("TabMenuButtonStyle");
            btnClients.Style = (Style)FindResource("TabMenuButtonStyle");
            btnVehicles.Style = (Style)FindResource("TabMenuButtonStyle");
            btnWaterSources.Style = (Style)FindResource("TabMenuButtonStyle");
            btnProducts.Style = (Style)FindResource("TabMenuButtonStyle");
            btnHandbook.Style = (Style)FindResource("TabMenuButtonStyle");
            btnReports.Style = (Style)FindResource("TabMenuButtonStyle");

            foreach (var button in new[] { btnOrders, btnClients, btnVehicles, btnWaterSources, btnProducts, btnHandbook, btnReports })
            {
                button.Background = System.Windows.Media.Brushes.Transparent;
                button.Foreground = System.Windows.Media.Brushes.White;
            }
        }

        private void UpdatePageTitle(string pageName)
        {
            switch (pageName)
            {
                case "OrdersPage":
                    pageTitle.Text = "Заказы";
                    break;
                case "ClientsPage":
                    pageTitle.Text = "Клиенты";
                    break;
                case "VehiclesPage":
                    pageTitle.Text = "Транспорт";
                    break;
                case "WaterSourcesPage":
                    pageTitle.Text = "Источники воды";
                    break;
                case "ProductsPage":
                    pageTitle.Text = "Продукты";
                    break;
                case "ReportsPage":
                    pageTitle.Text = "Отчеты";
                    break;
                case "HandbookPage":
                    pageTitle.Text = "Справочники";
                    break;
                default:
                    pageTitle.Text = "АкваВита";
                    break;
            }
        }

        private void NavigateToPage(string pageName)
        {
            Page page = null;

            switch (pageName)
            {
                case "DashboardPage":
                    //page = new DashboardPage();
                    break;
                case "OrdersPage":
                    //page = new OrdersPage();
                    break;
                case "ClientsPage":
                    //page = new ClientsPage();
                    break;
                case "VehiclesPage":
                    //page = new VehiclesPage();
                    break;
                case "WaterSourcesPage":
                    //page = new WaterSourcesPage();
                    break;
                case "ProductsPage":
                    //page = new ProductsPage();
                    break;
                case "ReportsPage":
                    //page = new ReportsPage();
                    break;
                case "HandbookPage":
                    //page = new HandbookPage();
                    break;
                default:
                    //page = new DashboardPage();
                    break;
            }

            if (page != null)
            {
                MainFrame.Navigate(page);
            }
        }

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

        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
        {
            SwitchWindowState();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
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

        private void SwitchWindowState()
        {
            if (WindowState == WindowState.Normal)
                WindowState = WindowState.Maximized;
            else
                WindowState = WindowState.Normal;
        }
    }
}