using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WaterDelivery.Data;
using WaterDelivery.Views.Windows;

namespace WaterDelivery.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для VehiclesPage.xaml
    /// </summary>
    public partial class VehiclesPage : Page
    {
        // Используем ObservableCollection для автоматического обновления UI
        private ObservableCollection<Vehicle> _vehicles;
        private List<Vehicle> _allVehicles; // Полный список транспорта до фильтрации

        public VehiclesPage()
        {
            InitializeComponent();

            // Инициализация коллекций
            _vehicles = new ObservableCollection<Vehicle>();
            _allVehicles = new List<Vehicle>();

            // Привязка источника данных для DataGrid
            dgVehicles.ItemsSource = _vehicles;

            // Загрузка данных
            LoadVehicleData();

            // Загрузка справочников
            LoadReferences();
        }

        /// <summary>
        /// Загружает данные транспорта
        /// </summary>
        private async void LoadVehicleData()
        {
            try
            {
                await Task.Run(() =>
                {
                    using (var context = new WaterDeliveryEntities())
                    {
                        // Загружаем ТС с типами, моделями, статусами и гаражами
                        _allVehicles = context.Vehicle
                            .Include(v => v.VehicleType)
                            .Include(v => v.VehicleModel.VehicleBrand)
                            .Include(v => v.Status)
                            .Include(v => v.Garage)
                            .ToList();
                    }
                });

                // Обновляем UI
                UpdateVehiclesList(_allVehicles);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных транспорта: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Обновляет список транспорта в UI
        /// </summary>
        private void UpdateVehiclesList(List<Vehicle> vehicles)
        {
            _vehicles.Clear();
            foreach (var vehicle in vehicles)
            {
                _vehicles.Add(vehicle);
            }

            // Обновляем счетчик
            txtVehicleCount.Text = $"Всего: {_vehicles.Count} единиц";
        }

        /// <summary>
        /// Загружает справочники (типы транспорта, статусы)
        /// </summary>
        private void LoadReferences()
        {
            try
            {
                using (var context = new WaterDeliveryEntities())
                {
                    // Загружаем типы транспорта
                    var vehicleTypes = context.VehicleType.ToList();

                    // Добавляем пункт "Все типы"
                    var allTypes = new VehicleType { VehicleTypeId = 0, TypeName = "Все типы" };
                    vehicleTypes.Insert(0, allTypes);

                    cmbVehicleType.ItemsSource = vehicleTypes;
                    cmbVehicleType.DisplayMemberPath = "TypeName";
                    cmbVehicleType.SelectedIndex = 0;

                    // Загружаем статусы
                    // Для транспорта используются записи из таблицы Status, где StatusTypeId = 3
                    var statuses = context.Status
                        .Where(s => s.StatusTypeId == 3)  // 3 - тип статуса для транспорта
                        .ToList();

                    // Добавляем пункт "Все статусы"
                    var allStatuses = new Status { StatusId = 0, StatusName = "Все статусы" };
                    statuses.Insert(0, allStatuses);

                    cmbStatus.ItemsSource = statuses;
                    cmbStatus.DisplayMemberPath = "StatusName";
                    cmbStatus.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке справочников: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Применяет фильтры к списку транспорта
        /// </summary>
        private void ApplyFilters()
        {
            int selectedTypeId = 0;
            int selectedStatusId = 0;
            string searchText = txtSearch.Text.ToLower().Trim();

            // Получаем выбранный тип транспорта
            if (cmbVehicleType.SelectedItem is VehicleType selectedType)
            {
                selectedTypeId = selectedType.VehicleTypeId;
            }

            // Получаем выбранный статус
            if (cmbStatus.SelectedItem is Status selectedStatus)
            {
                selectedStatusId = selectedStatus.StatusId;
            }

            // Фильтруем транспорт
            var filteredVehicles = _allVehicles.AsEnumerable();

            // Фильтр по типу транспорта
            if (selectedTypeId > 0)
            {
                filteredVehicles = filteredVehicles.Where(v => v.VehicleTypeId == selectedTypeId);
            }

            // Фильтр по статусу
            if (selectedStatusId > 0)
            {
                filteredVehicles = filteredVehicles.Where(v => v.StatusId == selectedStatusId);
            }

            // Фильтр по строке поиска
            if (!string.IsNullOrEmpty(searchText))
            {
                filteredVehicles = filteredVehicles.Where(v =>
                    (v.LicensePlate != null && v.LicensePlate.ToLower().Contains(searchText)) ||
                    (v.VIN != null && v.VIN.ToLower().Contains(searchText)) ||
                    (v.VehicleModel.VehicleBrand.BrandName.ToLower().Contains(searchText)) ||
                    (v.VehicleModel.ModelName.ToLower().Contains(searchText))
                );
            }

            // Обновляем список
            UpdateVehiclesList(filteredVehicles.ToList());
        }

        /// <summary>
        /// Добавляет новый транспорт
        /// </summary>
        private void AddNewVehicle()
        {
            var dialog = new VehicleDialog();
            if (dialog.ShowDialog() == true)
            {
                // Перезагружаем данные
                LoadVehicleData();
            }
        }

        /// <summary>
        /// Редактирует выбранный транспорт
        /// </summary>
        private void EditVehicle(Vehicle vehicle)
        {
            if (vehicle != null)
            {
                var dialog = new VehicleDialog(vehicle.VehicleId);
                if (dialog.ShowDialog() == true)
                {
                    // Перезагружаем данные
                    LoadVehicleData();
                }
            }
        }

        /// <summary>
        /// Удаляет выбранный транспорт
        /// </summary>
        private void DeleteVehicle(Vehicle vehicle)
        {
            if (vehicle == null) return;

            // Запрашиваем подтверждение
            string displayName = $"{vehicle.VehicleModel.VehicleBrand.BrandName} {vehicle.VehicleModel.ModelName} ({vehicle.LicensePlate})";

            var result = MessageBox.Show(
                $"Вы действительно хотите удалить транспортное средство '{displayName}'?",
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
                        if (context.Order.Any(o => o.AssignedVehicleId == vehicle.VehicleId))
                        {
                            MessageBox.Show(
                                "Невозможно удалить транспортное средство, так как оно используется в заказах.",
                                "Ошибка удаления",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                            return;
                        }

                        // Получаем транспортное средство из БД
                        var dbVehicle = context.Vehicle.Find(vehicle.VehicleId);
                        if (dbVehicle != null)
                        {
                            // Удаляем транспортное средство
                            context.Vehicle.Remove(dbVehicle);
                            context.SaveChanges();

                            // Обновляем список
                            _allVehicles.Remove(vehicle);
                            _vehicles.Remove(vehicle);
                            txtVehicleCount.Text = $"Всего: {_vehicles.Count} единиц";

                            MessageBox.Show(
                                "Транспортное средство успешно удалено.",
                                "Удаление транспорта",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Ошибка при удалении транспортного средства: " + ex.Message,
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        #region Обработчики событий

        private void BtnAddVehicle_Click(object sender, RoutedEventArgs e)
        {
            AddNewVehicle();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            // Сбрасываем фильтры
            cmbVehicleType.SelectedIndex = 0;
            cmbStatus.SelectedIndex = 0;
            txtSearch.Clear();

            // Показываем все транспортные средства
            UpdateVehiclesList(_allVehicles);
        }

        private void CmbVehicleType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void CmbStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Vehicle vehicle)
            {
                EditVehicle(vehicle);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Vehicle vehicle)
            {
                DeleteVehicle(vehicle);
            }
        }

        #endregion
    }
}