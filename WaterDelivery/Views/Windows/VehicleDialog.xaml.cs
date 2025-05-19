using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data.Entity;
using System.Linq;
using WaterDelivery.Data;
using SchoolApp.Models;

namespace WaterDelivery.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для VehicleDialog.xaml
    /// </summary>
    public partial class VehicleDialog : Window
    {
        private int _vehicleId = 0;
        private bool _isEditMode = false;
        private Vehicle _vehicle;

        /// <summary>
        /// Конструктор для создания нового транспортного средства
        /// </summary>
        public VehicleDialog()
        {
            InitializeComponent();

            _isEditMode = false;
            txtTitle.Text = "Новое транспортное средство";

            LoadLookupData();
            SetDefaultValues();
        }

        /// <summary>
        /// Конструктор для редактирования существующего транспортного средства
        /// </summary>
        public VehicleDialog(int vehicleId)
        {
            InitializeComponent();

            _vehicleId = vehicleId;
            _isEditMode = true;
            txtTitle.Text = "Редактирование транспортного средства";

            LoadLookupData();
            LoadVehicleData();
        }

        /// <summary>
        /// Устанавливает значения по умолчанию для нового транспортного средства
        /// </summary>
        private void SetDefaultValues()
        {
            // Устанавливаем текущий год по умолчанию
            txtYear.Text = DateTime.Now.Year.ToString();

            // Устанавливаем дату приобретения на текущую дату
            dpAcquisitionDate.SelectedDate = DateTime.Today;

            // Дизель по умолчанию, если это цистерна или грузовик
            if (cmbVehicleType.SelectedIndex >= 0 && cmbVehicleType.SelectedIndex <= 1)
            {
                txtFuelType.Text = "Дизель";
            }
            else
            {
                txtFuelType.Text = "Бензин";
            }

            // По умолчанию статус "В эксплуатации"
            foreach (Status status in cmbStatus.Items)
            {
                if (status.StatusName == "В эксплуатации")
                {
                    cmbStatus.SelectedItem = status;
                    break;
                }
            }
        }

        /// <summary>
        /// Загружает справочные данные (типы транспорта, марки, модели, статусы)
        /// </summary>
        private void LoadLookupData()
        {
            try
            {
                using (var context = new WaterDeliveryEntities())
                {
                    // Загрузка типов транспорта
                    var vehicleTypes = context.VehicleType.ToList();
                    cmbVehicleType.ItemsSource = vehicleTypes;
                    cmbVehicleType.SelectedValuePath = "VehicleTypeId";

                    // Загрузка статусов транспорта (StatusTypeId = 3 для транспорта)
                    var statuses = context.Status
                        .Where(s => s.StatusTypeId == 3)
                        .ToList();
                    cmbStatus.ItemsSource = statuses;
                    cmbStatus.SelectedValuePath = "StatusId";

                    // Загрузка марок транспорта
                    var brands = context.VehicleBrand.ToList();
                    cmbBrand.ItemsSource = brands;
                    cmbBrand.SelectedValuePath = "BrandId";

                    // Загрузка гаражей
                    var garages = context.Garage.ToList();
                    cmbGarage.ItemsSource = garages;
                    cmbGarage.SelectedValuePath = "GarageId";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке справочных данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Загружает данные транспортного средства для редактирования
        /// </summary>
        private void LoadVehicleData()
        {
            try
            {
                using (var context = new WaterDeliveryEntities())
                {
                    // Загружаем данные транспортного средства
                    _vehicle = context.Vehicle
                        .Include(v => v.VehicleType)
                        .Include(v => v.VehicleModel.VehicleBrand)
                        .Include(v => v.Status)
                        .Include(v => v.Garage)
                        .FirstOrDefault(v => v.VehicleId == _vehicleId);

                    if (_vehicle != null)
                    {
                        // Заполняем поля формы
                        cmbVehicleType.SelectedValue = _vehicle.VehicleTypeId;
                        cmbStatus.SelectedValue = _vehicle.StatusId;

                        // Выбираем бренд и загружаем соответствующие модели
                        cmbBrand.SelectedValue = _vehicle.VehicleModel.BrandId;
                        LoadModels(_vehicle.VehicleModel.BrandId);
                        cmbModel.SelectedValue = _vehicle.ModelId;

                        txtLicensePlate.Text = _vehicle.LicensePlate;
                        txtYear.Text = _vehicle.Year.ToString();
                        txtVIN.Text = _vehicle.VIN;
                        txtFuelType.Text = _vehicle.FuelType;

                        if (_vehicle.TankCapacity.HasValue)
                            txtTankCapacity.Text = _vehicle.TankCapacity.Value.ToString();

                        if (_vehicle.CurrentMileage.HasValue)
                            txtCurrentMileage.Text = _vehicle.CurrentMileage.Value.ToString();

                        if (_vehicle.GarageId.HasValue)
                            cmbGarage.SelectedValue = _vehicle.GarageId.Value;

                        dpAcquisitionDate.SelectedDate = _vehicle.AcquisitionDate;
                        dpDisposalDate.SelectedDate = _vehicle.DisposalDate;

                        txtNotes.Text = _vehicle.Notes;
                    }
                    else
                    {
                        MessageBox.Show("Транспортное средство не найдено", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных транспортного средства: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Загружает модели для выбранной марки
        /// </summary>
        private void LoadModels(int brandId)
        {
            try
            {
                using (var context = new WaterDeliveryEntities())
                {
                    var models = context.VehicleModel
                        .Where(m => m.BrandId == brandId)
                        .ToList();
                    cmbModel.ItemsSource = models;
                    cmbModel.SelectedValuePath = "ModelId";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке моделей: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Проверяет заполнение обязательных полей
        /// </summary>
        private bool ValidateForm()
        {
            // Проверка основных полей
            if (cmbVehicleType.SelectedItem == null)
            {
                ShowError("Необходимо выбрать тип транспортного средства");
                return false;
            }

            if (cmbStatus.SelectedItem == null)
            {
                ShowError("Необходимо выбрать статус");
                return false;
            }

            if (cmbBrand.SelectedItem == null)
            {
                ShowError("Необходимо выбрать марку");
                return false;
            }

            if (cmbModel.SelectedItem == null)
            {
                ShowError("Необходимо выбрать модель");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtLicensePlate.Text))
            {
                ShowError("Необходимо указать государственный номер");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtYear.Text))
            {
                ShowError("Необходимо указать год выпуска");
                return false;
            }

            // Проверка числовых значений
            if (!int.TryParse(txtYear.Text, out int year) || year < 1950 || year > DateTime.Now.Year + 1)
            {
                ShowError($"Год выпуска должен быть числом от 1950 до {DateTime.Now.Year + 1}");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(txtTankCapacity.Text) &&
                (!decimal.TryParse(txtTankCapacity.Text, out decimal tankCapacity) || tankCapacity <= 0))
            {
                ShowError("Объем бака должен быть положительным числом");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(txtCurrentMileage.Text) &&
                (!int.TryParse(txtCurrentMileage.Text, out int mileage) || mileage < 0))
            {
                ShowError("Пробег должен быть неотрицательным числом");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Отображает сообщение об ошибке
        /// </summary>
        private void ShowError(string message)
        {
            txtError.Text = message;
            txtError.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Скрывает сообщение об ошибке
        /// </summary>
        private void HideError()
        {
            txtError.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Сохраняет данные транспортного средства
        /// </summary>
        private void SaveVehicle()
        {
            try
            {
                using (var context = new WaterDeliveryEntities())
                {
                    Vehicle vehicle;

                    // Получаем существующее транспортное средство или создаем новое
                    if (_isEditMode)
                    {
                        vehicle = context.Vehicle.Find(_vehicleId);
                    }
                    else
                    {
                        vehicle = new Vehicle
                        {
                            CreatedBy = CurrentUser.UserId
                        };
                        context.Vehicle.Add(vehicle);
                    }

                    // Заполняем данные транспортного средства
                    vehicle.VehicleTypeId = (int)cmbVehicleType.SelectedValue;
                    vehicle.StatusId = (int)cmbStatus.SelectedValue;
                    vehicle.ModelId = (int)cmbModel.SelectedValue;
                    vehicle.LicensePlate = txtLicensePlate.Text.Trim();
                    vehicle.Year = int.Parse(txtYear.Text);
                    vehicle.VIN = txtVIN.Text.Trim();
                    vehicle.FuelType = txtFuelType.Text.Trim();

                    if (!string.IsNullOrWhiteSpace(txtTankCapacity.Text))
                        vehicle.TankCapacity = decimal.Parse(txtTankCapacity.Text);
                    else
                        vehicle.TankCapacity = null;

                    if (!string.IsNullOrWhiteSpace(txtCurrentMileage.Text))
                        vehicle.CurrentMileage = int.Parse(txtCurrentMileage.Text);
                    else
                        vehicle.CurrentMileage = null;

                    if (cmbGarage.SelectedValue != null)
                        vehicle.GarageId = (int)cmbGarage.SelectedValue;
                    else
                        vehicle.GarageId = null;

                    vehicle.AcquisitionDate = dpAcquisitionDate.SelectedDate;
                    vehicle.DisposalDate = dpDisposalDate.SelectedDate;
                    vehicle.Notes = txtNotes.Text.Trim();

                    // Сохраняем изменения
                    context.SaveChanges();

                    DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных транспортного средства: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Обработчики событий

        /// <summary>
        /// Обработчик изменения марки транспортного средства
        /// </summary>
        private void CmbBrand_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbBrand.SelectedValue != null)
            {
                int brandId = (int)cmbBrand.SelectedValue;
                LoadModels(brandId);
            }
        }

        /// <summary>
        /// Обработчик нажатия кнопки сохранения
        /// </summary>
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            HideError();

            if (ValidateForm())
            {
                SaveVehicle();
            }
        }

        /// <summary>
        /// Обработчик нажатия кнопки отмены
        /// </summary>
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        /// <summary>
        /// Обработчик нажатия кнопки закрытия
        /// </summary>
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        /// <summary>
        /// Обработчик перетаскивания окна
        /// </summary>
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        #endregion
    }
}