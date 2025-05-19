using System;
using System.Windows;
using System.Windows.Controls;
using System.Data.Entity;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Media;
using WaterDelivery.Data;
using WaterDelivery.Views.Windows;
using System.Collections.ObjectModel;

namespace WaterDelivery.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для ClientDetailsPage.xaml
    /// </summary>
    public partial class ClientDetailsPage : Page
    {
        private int _clientId;
        private Client _client;

        // Коллекции для хранения данных о контактах, истории баллов и заказах
        private ObservableCollection<ClientContact> _contacts;
        private ObservableCollection<LoyaltyPointMovement> _pointMovements;
        private ObservableCollection<Order> _orders;

        public ClientDetailsPage(int clientId)
        {
            InitializeComponent();

            _clientId = clientId;

            // Инициализация коллекций
            _contacts = new ObservableCollection<ClientContact>();
            _pointMovements = new ObservableCollection<LoyaltyPointMovement>();
            _orders = new ObservableCollection<Order>();

            // Привязка коллекций к элементам UI
            lvContacts.ItemsSource = _contacts;
            dgLoyaltyHistory.ItemsSource = _pointMovements;
            dgOrders.ItemsSource = _orders;

            // Загрузка данных
            LoadClientData();
        }

        /// <summary>
        /// Загружает данные клиента
        /// </summary>
        private void LoadClientData()
        {
            try
            {
                using (var context = new WaterDeliveryEntities())
                {
                    // Загружаем клиента со всеми необходимыми данными
                    _client = context.Client
                        .Include(c => c.ClientType)
                        .Include(c => c.DeliveryArea)
                        .FirstOrDefault(c => c.ClientId == _clientId);

                    if (_client != null)
                    {
                        // Обновляем заголовок страницы
                        Title = $"Клиент: {GetDisplayName(_client)}";

                        // Заполняем информацию о клиенте
                        UpdateClientInfo();

                        // Загружаем контакты клиента
                        LoadClientContacts(context);

                        // Загружаем историю бонусных баллов
                        LoadLoyaltyPointHistory(context);

                        // Загружаем историю заказов
                        LoadOrderHistory(context);
                    }
                    else
                    {
                        MessageBox.Show("Клиент не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        NavigationService.GoBack();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных клиента: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Получает отображаемое имя клиента
        /// </summary>
        private string GetDisplayName(Client client)
        {
            if (!string.IsNullOrEmpty(client.CompanyName))
            {
                return client.CompanyName;
            }
            else
            {
                string fullName = $"{client.LastName} {client.FirstName}";
                if (!string.IsNullOrEmpty(client.MiddleName))
                {
                    fullName += $" {client.MiddleName}";
                }
                return fullName.Trim();
            }
        }

        /// <summary>
        /// Обновляет информацию о клиенте на странице
        /// </summary>
        private void UpdateClientInfo()
        {
            // Заголовок
            txtClientName.Text = GetDisplayName(_client);
            txtClientType.Text = $"Тип: {_client.ClientType.TypeName}";
            txtRegistrationDate.Text = $"Регистрация: {_client.RegistrationDate:dd.MM.yyyy}";

            // Основная информация
            txtClientId.Text = _client.ClientId.ToString();

            // ФИО
            txtFullName.Text = $"{_client.LastName} {_client.FirstName} {_client.MiddleName}".Trim();
            txtLastName.Text = string.IsNullOrEmpty(_client.LastName) ? "-" : _client.LastName;
            txtFirstName.Text = string.IsNullOrEmpty(_client.FirstName) ? "-" : _client.FirstName;
            txtMiddleName.Text = string.IsNullOrEmpty(_client.MiddleName) ? "-" : _client.MiddleName;

            // Контактная информация
            txtPhone.Text = string.IsNullOrEmpty(_client.Phone) ? "-" : _client.Phone;
            txtEmail.Text = string.IsNullOrEmpty(_client.Email) ? "-" : _client.Email;
            txtDeliveryArea.Text = _client.DeliveryArea.AreaName;
            txtLegalAddress.Text = string.IsNullOrEmpty(_client.LegalAddress) ? "-" : _client.LegalAddress;
            txtINN.Text = string.IsNullOrEmpty(_client.INN) ? "-" : _client.INN;
            txtLoyaltyPoints.Text = _client.LoyaltyPoints.ToString();

            // Дополнительная информация
            txtRegistrationDateDetailed.Text = _client.RegistrationDate.ToString("dd.MM.yyyy HH:mm:ss");
            txtClientTypeDetailed.Text = _client.ClientType.TypeName;

            // Примечания
            txtNotes.Text = string.IsNullOrEmpty(_client.Notes) ? "-" : _client.Notes;

            // Информация о компании
            if (!string.IsNullOrEmpty(_client.CompanyName))
            {
                cardCompanyInfo.Visibility = Visibility.Visible;
                txtCompanyName.Text = _client.CompanyName;
                txtKPP.Text = string.IsNullOrEmpty(_client.KPP) ? "-" : _client.KPP;
                txtOGRN.Text = string.IsNullOrEmpty(_client.OGRN) ? "-" : _client.OGRN;
                txtBankName.Text = string.IsNullOrEmpty(_client.BankName) ? "-" : _client.BankName;
                txtBankBIC.Text = string.IsNullOrEmpty(_client.BankBIC) ? "-" : _client.BankBIC;
                txtBankAccount.Text = string.IsNullOrEmpty(_client.BankAccount) ? "-" : _client.BankAccount;
                txtCorrespondentAccount.Text = string.IsNullOrEmpty(_client.CorrespondentAccount) ? "-" : _client.CorrespondentAccount;
            }
            else
            {
                cardCompanyInfo.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Загружает контакты клиента
        /// </summary>
        private void LoadClientContacts(WaterDeliveryEntities context)
        {
            try
            {
                var contacts = context.ClientContact
                    .Where(c => c.ClientId == _clientId)
                    .ToList();

                // Обновляем коллекцию контактов
                _contacts.Clear();
                foreach (var contact in contacts)
                {

                    _contacts.Add(contact);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке контактов: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Загружает историю бонусных баллов
        /// </summary>
        private void LoadLoyaltyPointHistory(WaterDeliveryEntities context)
        {
            try
            {
                var movements = context.LoyaltyPointMovement
                    .Include(m => m.LoyaltyPointMovementType)
                    .Where(m => m.ClientId == _clientId)
                    .OrderByDescending(m => m.MovementDate)
                    .ToList();

                // Обновляем коллекцию движений баллов
                _pointMovements.Clear();
                foreach (var movement in movements)
                {
                    _pointMovements.Add(movement);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке истории баллов: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Загружает историю заказов
        /// </summary>
        private void LoadOrderHistory(WaterDeliveryEntities context)
        {
            try
            {
                var orders = context.Order
                    .Include(o => o.Status)
                    .Where(o => o.ClientId == _clientId)
                    .OrderByDescending(o => o.OrderDate)
                    .ToList();

                // Обновляем коллекцию заказов
                _orders.Clear();
                foreach (var order in orders)
                {
                    _orders.Add(order);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке истории заказов: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Открывает диалог редактирования клиента
        /// </summary>
        private void EditClient()
        {
            var dialog = new ClientDialog(_clientId);
            if (dialog.ShowDialog() == true)
            {
                // Перезагружаем данные клиента
                LoadClientData();
            }
        }

        /// <summary>
        /// Удаляет клиента
        /// </summary>
        private void DeleteClient()
        {
            // Запрашиваем подтверждение
            var result = MessageBox.Show(
                $"Вы действительно хотите удалить клиента '{GetDisplayName(_client)}'?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var context = new WaterDeliveryEntities())
                    {
                        // Проверяем, есть ли связанные заказы
                        if (context.Order.Any(o => o.ClientId == _clientId))
                        {
                            MessageBox.Show(
                                "Невозможно удалить клиента, так как у него есть связанные заказы.",
                                "Ошибка удаления",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                            return;
                        }

                        // Получаем клиента из БД
                        var client = context.Client.Find(_clientId);
                        if (client != null)
                        {
                            // Удаляем связанные контакты
                            var contacts = context.ClientContact.Where(c => c.ClientId == _clientId).ToList();
                            foreach (var contact in contacts)
                            {
                                context.ClientContact.Remove(contact);
                            }

                            // Удаляем связанные движения баллов
                            var pointMovements = context.LoyaltyPointMovement.Where(m => m.ClientId == _clientId).ToList();
                            foreach (var movement in pointMovements)
                            {
                                context.LoyaltyPointMovement.Remove(movement);
                            }

                            // Удаляем клиента
                            context.Client.Remove(client);
                            context.SaveChanges();

                            MessageBox.Show(
                                "Клиент успешно удален.",
                                "Удаление клиента",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

                            // Возвращаемся на предыдущую страницу
                            NavigationService.GoBack();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Ошибка при удалении клиента: " + ex.Message,
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Открывает диалог управления бонусными баллами
        /// </summary>
        private void ManageLoyaltyPoints()
        {
            var dialog = new LoyaltyPointsDialog(_clientId, _client.LoyaltyPoints);
            if (dialog.ShowDialog() == true)
            {
                // Перезагружаем данные клиента
                LoadClientData();
            }
        }

        /// <summary>
        /// Добавляет новый контакт
        /// </summary>
        private void AddContact()
        {
            var dialog = new ContactDialog(_clientId);
            if (dialog.ShowDialog() == true)
            {
                LoadClientData();
            }


        }

        /// <summary>
        /// Редактирует выбранный контакт
        /// </summary>
        private void EditContact(ClientContact contact)
        {
            var dialog = new ContactDialog(contact);
            if (dialog.ShowDialog() == true)
            {
                LoadClientData();
            }
        }

        /// <summary>
        /// Удаляет выбранный контакт
        /// </summary>
        private void DeleteContact(ClientContact contact)
        {
            // Формируем отображаемое имя контакта с учетом возможных null-значений
            string displayName = contact.LastName;

            if (!string.IsNullOrEmpty(contact.FirstName))
            {
                displayName += " " + contact.FirstName[0] + ".";
            }

            if (!string.IsNullOrEmpty(contact.MiddleName))
            {
                displayName += " " + contact.MiddleName[0] + ".";
            }

            // Запрашиваем подтверждение
            var result = MessageBox.Show(
                $"Вы действительно хотите удалить контакт '{displayName}'?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var context = new WaterDeliveryEntities())
                    {
                        var dbContact = context.ClientContact.Find(contact.ContactId);
                        if (dbContact != null)
                        {
                            context.ClientContact.Remove(dbContact);
                            context.SaveChanges();

                            // Обновляем список контактов
                            _contacts.Remove(contact);

                            MessageBox.Show(
                                "Контакт успешно удален.",
                                "Удаление контакта",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Ошибка при удалении контакта: " + ex.Message,
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Создает новый заказ для клиента
        /// </summary>
        private void AddOrder()
        {
            // Здесь будет открытие формы создания заказа
            MessageBox.Show("Функция создания заказа в разработке", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Просматривает детали заказа
        /// </summary>
        private void ViewOrder(Order order)
        {
            // Здесь будет открытие формы просмотра заказа
        }

        #region Обработчики событий

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            EditClient();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            DeleteClient();
        }

        private void BtnManageLoyaltyPoints_Click(object sender, RoutedEventArgs e)
        {
            ManageLoyaltyPoints();
        }

        private void BtnAddContact_Click(object sender, RoutedEventArgs e)
        {
            AddContact();
        }

        private void BtnEditContact_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is ClientContact contact)
            {
                EditContact(contact);
            }
        }

        private void BtnDeleteContact_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is ClientContact contact)
            {
                DeleteContact(contact);
            }
        }

        private void BtnAddOrder_Click(object sender, RoutedEventArgs e)
        {
            AddOrder();
        }

        private void BtnViewOrder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Order order)
            {
                ViewOrder(order);
            }
        }

        private void DgOrders_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (dgOrders.SelectedItem is Order order)
            {
                ViewOrder(order);
            }
        }

        #endregion
    }
}