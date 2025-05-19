using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data.Entity;
using WaterDelivery.Data;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using WaterDelivery.Views.Windows;

namespace WaterDelivery.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для ClientsPage.xaml
    /// </summary>
    public partial class ClientsPage : Page
    {
        // Используем ObservableCollection для автоматического обновления UI
        private ObservableCollection<Client> _clients;
        private List<Client> _allClients; // Полный список клиентов до фильтрации

        public ClientsPage()
        {
            InitializeComponent();

            // Инициализация коллекций
            _clients = new ObservableCollection<Client>();
            _allClients = new List<Client>();

            // Привязка источника данных для DataGrid
            dgClients.ItemsSource = _clients;

            // Загрузка данных
            LoadClientData();

            // Загрузка справочников
            LoadReferences();
        }

        /// <summary>
        /// Загружает данные клиентов
        /// </summary>
        private async void LoadClientData()
        {
            try
            {

                await Task.Run(() =>
                {
                    using (var context = new WaterDeliveryEntities())
                    {
                        // Загружаем клиентов с типами и зонами доставки
                        _allClients = context.Client
                            .Include(c => c.ClientType)
                            .Include(c => c.DeliveryArea)
                            .ToList();
                    }
                });

                // Обновляем UI
                UpdateClientsList(_allClients);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке клиентов: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
            }
        }

        /// <summary>
        /// Обновляет список клиентов в UI
        /// </summary>
        private void UpdateClientsList(List<Client> clients)
        {
            _clients.Clear();
            foreach (var client in clients)
            {
                _clients.Add(client);
            }

            // Обновляем счетчик
            txtClientCount.Text = $"Всего: {_clients.Count} клиентов";
        }

        /// <summary>
        /// Загружает справочники (типы клиентов, зоны доставки)
        /// </summary>
        private void LoadReferences()
        {
            try
            {
                using (var context = new WaterDeliveryEntities())
                {
                    // Загружаем типы клиентов
                    var clientTypes = context.ClientType.ToList();

                    // Добавляем пункт "Все типы"
                    var allTypes = new ClientType { ClientTypeId = 0, TypeName = "Все типы" };
                    clientTypes.Insert(0, allTypes);

                    cmbClientType.ItemsSource = clientTypes;
                    cmbClientType.DisplayMemberPath = "TypeName";
                    cmbClientType.SelectedIndex = 0;

                    // Загружаем зоны доставки
                    var deliveryAreas = context.DeliveryArea.ToList();

                    // Добавляем пункт "Все зоны"
                    var allAreas = new DeliveryArea { DeliveryAreaId = 0, AreaName = "Все зоны" };
                    deliveryAreas.Insert(0, allAreas);

                    cmbDeliveryArea.ItemsSource = deliveryAreas;
                    cmbDeliveryArea.DisplayMemberPath = "AreaName";
                    cmbDeliveryArea.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке справочников: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Применяет фильтры к списку клиентов
        /// </summary>
        private void ApplyFilters()
        {
            int selectedTypeId = 0;
            int selectedAreaId = 0;
            string searchText = txtSearch.Text.ToLower().Trim();

            // Получаем выбранный тип клиента
            if (cmbClientType.SelectedItem is ClientType selectedType)
            {
                selectedTypeId = selectedType.ClientTypeId;
            }

            // Получаем выбранную зону доставки
            if (cmbDeliveryArea.SelectedItem is DeliveryArea selectedArea)
            {
                selectedAreaId = selectedArea.DeliveryAreaId;
            }

            // Фильтруем клиентов
            var filteredClients = _allClients.AsEnumerable();

            // Фильтр по типу клиента
            if (selectedTypeId > 0)
            {
                filteredClients = filteredClients.Where(c => c.ClientTypeId == selectedTypeId);
            }

            // Фильтр по зоне доставки
            if (selectedAreaId > 0)
            {
                filteredClients = filteredClients.Where(c => c.DeliveryAreaId == selectedAreaId);
            }

            // Фильтр по строке поиска
            if (!string.IsNullOrEmpty(searchText))
            {
                filteredClients = filteredClients.Where(c =>
                    (c.FirstName != null && c.FirstName.ToLower().Contains(searchText)) ||
                    (c.LastName != null && c.LastName.ToLower().Contains(searchText)) ||
                    (c.MiddleName != null && c.MiddleName.ToLower().Contains(searchText)) ||
                    (c.CompanyName != null && c.CompanyName.ToLower().Contains(searchText)) ||
                    (c.Email != null && c.Email.ToLower().Contains(searchText)) ||
                    (c.Phone != null && c.Phone.ToLower().Contains(searchText))
                );
            }

            // Обновляем список
            UpdateClientsList(filteredClients.ToList());
        }

        /// <summary>
        /// Добавляет нового клиента
        /// </summary>
        private void AddNewClient()
        {
            var dialog = new ClientDialog();
            if (dialog.ShowDialog() == true)
            {
                // Перезагружаем данные
                LoadClientData();
            }
        }

        /// <summary>
        /// Редактирует выбранного клиента
        /// </summary>
        private void EditClient(Client client)
        {
            if (client != null)
            {
                var dialog = new ClientDialog(client.ClientId);
                if (dialog.ShowDialog() == true)
                {
                    // Перезагружаем данные
                    LoadClientData();
                }
            }
        }

        /// <summary>
        /// Удаляет выбранного клиента
        /// </summary>
        private void DeleteClient(Client client)
        {
            if (client == null) return;

            // Запрашиваем подтверждение
            string displayName = !string.IsNullOrEmpty(client.CompanyName)
                ? client.CompanyName
                : $"{client.LastName} {client.FirstName} {client.MiddleName}".Trim();

            var result = MessageBox.Show(
                $"Вы действительно хотите удалить клиента '{displayName}'?",
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
                        if (context.Order.Any(o => o.ClientId == client.ClientId))
                        {
                            MessageBox.Show(
                                "Невозможно удалить клиента, так как у него есть связанные заказы.",
                                "Ошибка удаления",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                            return;
                        }

                        // Получаем клиента из БД
                        var dbClient = context.Client.Find(client.ClientId);
                        if (dbClient != null)
                        {
                            // Удаляем связанные контакты
                            var contacts = context.ClientContact.Where(c => c.ClientId == client.ClientId).ToList();
                            foreach (var contact in contacts)
                            {
                                context.ClientContact.Remove(contact);
                            }

                            // Удаляем связанные движения баллов
                            var pointMovements = context.LoyaltyPointMovement.Where(m => m.ClientId == client.ClientId).ToList();
                            foreach (var movement in pointMovements)
                            {
                                context.LoyaltyPointMovement.Remove(movement);
                            }

                            // Удаляем клиента
                            context.Client.Remove(dbClient);
                            context.SaveChanges();

                            // Обновляем список
                            _allClients.Remove(client);
                            _clients.Remove(client);
                            txtClientCount.Text = $"Всего: {_clients.Count} клиентов";

                            MessageBox.Show(
                                "Клиент успешно удален.",
                                "Удаление клиента",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
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
        /// Показывает подробную информацию о клиенте
        /// </summary>
        private void ShowClientDetails(Client client)
        {
            if (client != null)
            {
                var detailsPage = new ClientDetailsPage(client.ClientId);
                NavigationService.Navigate(detailsPage);
            }
        }

        #region Обработчики событий

        private void BtnAddClient_Click(object sender, RoutedEventArgs e)
        {
            AddNewClient();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            // Сбрасываем фильтры
            cmbClientType.SelectedIndex = 0;
            cmbDeliveryArea.SelectedIndex = 0;
            txtSearch.Clear();

            // Показываем всех клиентов
            UpdateClientsList(_allClients);
        }

        private void CmbClientType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void CmbDeliveryArea_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void DgClients_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgClients.SelectedItem is Client client)
            {
                ShowClientDetails(client);
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Client client)
            {
                EditClient(client);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Client client)
            {
                DeleteClient(client);
            }
        }

        #endregion
    }
}