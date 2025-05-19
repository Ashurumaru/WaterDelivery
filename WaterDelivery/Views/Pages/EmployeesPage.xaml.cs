using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WaterDelivery.Data;
using WaterDelivery.Views.Windows;

namespace WaterDelivery.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для EmployeesPage.xaml
    /// </summary>
    public partial class EmployeesPage : Page
    {
        // Используем ObservableCollection для автоматического обновления UI
        private ObservableCollection<Employee> _employees;
        private List<Employee> _allEmployees; // Полный список сотрудников до фильтрации

        public EmployeesPage()
        {
            InitializeComponent();

            // Инициализация коллекций
            _employees = new ObservableCollection<Employee>();
            _allEmployees = new List<Employee>();

            // Привязка источника данных для DataGrid
            dgEmployees.ItemsSource = _employees;

            // Загрузка данных
            LoadEmployeeData();

            // Загрузка справочников
            LoadReferences();
        }

        /// <summary>
        /// Загружает данные сотрудников
        /// </summary>
        private async void LoadEmployeeData()
        {
            try
            {
                await Task.Run(() =>
                {
                    using (var context = new WaterDeliveryEntities())
                    {
                        // Загружаем сотрудников с должностями
                        _allEmployees = context.Employee
                            .Include(e => e.EmployeePosition)
                            .Include(e => e.User)
                            .ToList();
                    }
                });

                // Обновляем UI
                UpdateEmployeesList(_allEmployees);

                // По умолчанию показываем только работающих сотрудников
                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных сотрудников: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Обновляет список сотрудников в UI
        /// </summary>
        private void UpdateEmployeesList(List<Employee> employees)
        {
            _employees.Clear();
            foreach (var employee in employees)
            {
                _employees.Add(employee);
            }

            // Обновляем счетчик
            txtEmployeeCount.Text = $"Всего: {_employees.Count} сотрудников";
        }

        /// <summary>
        /// Загружает справочники (должности)
        /// </summary>
        private void LoadReferences()
        {
            try
            {
                using (var context = new WaterDeliveryEntities())
                {
                    // Загружаем должности
                    var positions = context.EmployeePosition.ToList();

                    // Добавляем пункт "Все должности"
                    var allPositions = new EmployeePosition { PositionId = 0, PositionName = "Все должности" };
                    positions.Insert(0, allPositions);

                    cmbPosition.ItemsSource = positions;
                    cmbPosition.DisplayMemberPath = "PositionName";
                    cmbPosition.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке справочников: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Применяет фильтры к списку сотрудников
        /// </summary>
        private void ApplyFilters()
        { 
            if (txtSearch != null)
            {
                int selectedPositionId = 0;
                string selectedStatus = "active"; // По умолчанию показываем только работающих
                string searchText = txtSearch.Text.ToLower().Trim();

                // Получаем выбранную должность
                if (cmbPosition.SelectedItem is EmployeePosition selectedPosition)
                {
                    selectedPositionId = selectedPosition.PositionId;
                }

                // Получаем выбранный статус
                if (cmbStatus.SelectedItem is ComboBoxItem statusItem)
                {
                    selectedStatus = statusItem.Tag.ToString();
                }

                // Фильтруем сотрудников
                var filteredEmployees = _allEmployees.AsEnumerable();

                // Фильтр по должности
                if (selectedPositionId > 0)
                {
                    filteredEmployees = filteredEmployees.Where(e => e.PositionId == selectedPositionId);
                }

                // Фильтр по статусу (активные/уволенные)
                if (selectedStatus == "active")
                {
                    filteredEmployees = filteredEmployees.Where(e => e.FireDate == null);
                }
                else if (selectedStatus == "fired")
                {
                    filteredEmployees = filteredEmployees.Where(e => e.FireDate != null);
                }
                // Если selectedStatus == "all", то фильтрацию по статусу не применяем

                // Фильтр по строке поиска
                if (!string.IsNullOrEmpty(searchText))
                {
                    filteredEmployees = filteredEmployees.Where(e =>
                        (e.FirstName != null && e.FirstName.ToLower().Contains(searchText)) ||
                        (e.LastName != null && e.LastName.ToLower().Contains(searchText)) ||
                        (e.MiddleName != null && e.MiddleName.ToLower().Contains(searchText)) ||
                        (e.Phone != null && e.Phone.ToLower().Contains(searchText))
                    );
                }

                // Обновляем список
                UpdateEmployeesList(filteredEmployees.ToList());
            }
                
        }

        /// <summary>
        /// Добавляет нового сотрудника
        /// </summary>
        private void AddNewEmployee()
        {
            var dialog = new EmployeeDialog();
            if (dialog.ShowDialog() == true)
            {
                // Перезагружаем данные
                LoadEmployeeData();
            }
        }

        /// <summary>
        /// Редактирует выбранного сотрудника
        /// </summary>
        private void EditEmployee(Employee employee)
        {
            if (employee != null)
            {
                var dialog = new EmployeeDialog(employee.EmployeeId);
                if (dialog.ShowDialog() == true)
                {
                    // Перезагружаем данные
                    LoadEmployeeData();
                }
            }
        }

        /// <summary>
        /// Удаляет выбранного сотрудника
        /// </summary>
        private void DeleteEmployee(Employee employee)
        {
            if (employee == null) return;

            // Запрашиваем подтверждение
            string fullName = $"{employee.LastName} {employee.FirstName} {employee.MiddleName}".Trim();
            var result = MessageBox.Show(
                $"Вы действительно хотите удалить сотрудника '{fullName}'?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var context = new WaterDeliveryEntities())
                    {
                        // Проверяем, есть ли связанные записи
                        if (context.Order.Any(o => o.AssignedDriverId == employee.EmployeeId))
                        {
                            MessageBox.Show(
                                "Невозможно удалить сотрудника, так как он назначен на заказы.",
                                "Ошибка удаления",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                            return;
                        }

                        if (context.OrderItem.Any(oi => oi.AssignedEmployeeId == employee.EmployeeId))
                        {
                            MessageBox.Show(
                                "Невозможно удалить сотрудника, так как он назначен на позиции заказов.",
                                "Ошибка удаления",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                            return;
                        }

                        if (context.MovementProduct.Any(mp => mp.ResponsibleEmployeeId == employee.EmployeeId))
                        {
                            MessageBox.Show(
                                "Невозможно удалить сотрудника, так как он указан ответственным за перемещения товаров.",
                                "Ошибка удаления",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                            return;
                        }

                        // Получаем сотрудника из БД
                        var dbEmployee = context.Employee.Find(employee.EmployeeId);
                        if (dbEmployee != null)
                        {
                            // Удаляем учетную запись пользователя, если она есть
                            if (dbEmployee.UserId.HasValue)
                            {
                                var user = context.User.Find(dbEmployee.UserId.Value);
                                if (user != null)
                                {
                                    context.User.Remove(user);
                                }
                            }

                            // Удаляем сотрудника
                            context.Employee.Remove(dbEmployee);
                            context.SaveChanges();

                            // Обновляем список
                            _allEmployees.Remove(employee);
                            _employees.Remove(employee);
                            txtEmployeeCount.Text = $"Всего: {_employees.Count} сотрудников";

                            MessageBox.Show(
                                "Сотрудник успешно удален.",
                                "Удаление сотрудника",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Ошибка при удалении сотрудника: " + ex.Message,
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        #region Обработчики событий

        private void BtnAddEmployee_Click(object sender, RoutedEventArgs e)
        {
            AddNewEmployee();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            // Сбрасываем фильтры
            cmbPosition.SelectedIndex = 0;
            cmbStatus.SelectedIndex = 0; // "Все сотрудники"
            txtSearch.Clear();

            // Показываем всех сотрудников
            UpdateEmployeesList(_allEmployees);
        }

        private void CmbPosition_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
            if (sender is Button button && button.DataContext is Employee employee)
            {
                EditEmployee(employee);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Employee employee)
            {
                DeleteEmployee(employee);
            }
        }

        #endregion
    }
}