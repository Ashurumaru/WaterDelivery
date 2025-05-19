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
    /// Логика взаимодействия для EmployeeDialog.xaml
    /// </summary>
    public partial class EmployeeDialog : Window
    {
        private int _employeeId = 0;
        private bool _isEditMode = false;
        private Employee _employee;
        private User _user;

        /// <summary>
        /// Конструктор для создания нового сотрудника
        /// </summary>
        public EmployeeDialog()
        {
            InitializeComponent();

            _isEditMode = false;
            txtTitle.Text = "Новый сотрудник";

            LoadLookupData();
            SetDefaultValues();
        }

        /// <summary>
        /// Конструктор для редактирования существующего сотрудника
        /// </summary>
        public EmployeeDialog(int employeeId)
        {
            InitializeComponent();

            _employeeId = employeeId;
            _isEditMode = true;
            txtTitle.Text = "Редактирование сотрудника";

            LoadLookupData();
            LoadEmployeeData();
        }

        /// <summary>
        /// Устанавливает значения по умолчанию для нового сотрудника
        /// </summary>
        private void SetDefaultValues()
        {
            // Устанавливаем текущую дату найма по умолчанию
            dpHireDate.SelectedDate = DateTime.Today;

            // По умолчанию флажок создания учетной записи не установлен
            chkCreateUser.IsChecked = false;

            // По умолчанию предлагаем роль сотрудника в соответствии с должностью
            if (cmbUserRole.Items.Count > 0)
            {
                foreach (UserRole role in cmbUserRole.Items)
                {
                    if (role.RoleName.Contains("Сотрудник") || role.UserRoleId == 4)
                    {
                        cmbUserRole.SelectedItem = role;
                        break;
                    }
                }
            }

            // Управляем видимостью и доступностью полей учетной записи пользователя
            UpdateUserAccountVisibility();
        }

        /// <summary>
        /// Загружает справочные данные (должности, роли пользователей)
        /// </summary>
        private void LoadLookupData()
        {
            try
            {
                using (var context = new WaterDeliveryEntities())
                {
                    // Загрузка должностей
                    var positions = context.EmployeePosition.ToList();
                    cmbPosition.ItemsSource = positions;
                    cmbPosition.SelectedValuePath = "PositionId";

                    // Загрузка ролей пользователей
                    var userRoles = context.UserRole.ToList();
                    cmbUserRole.ItemsSource = userRoles;
                    cmbUserRole.SelectedValuePath = "UserRoleId";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке справочных данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Загружает данные сотрудника для редактирования
        /// </summary>
        private void LoadEmployeeData()
        {
            try
            {
                using (var context = new WaterDeliveryEntities())
                {
                    // Загружаем данные сотрудника
                    _employee = context.Employee
                        .Include(e => e.EmployeePosition)
                        .Include(e => e.User)
                        .FirstOrDefault(e => e.EmployeeId == _employeeId);

                    if (_employee != null)
                    {
                        // Заполняем поля формы
                        txtFirstName.Text = _employee.FirstName;
                        txtMiddleName.Text = _employee.MiddleName;
                        txtLastName.Text = _employee.LastName;
                        txtPhone.Text = _employee.Phone;
                        txtAddress.Text = _employee.Address;

                        cmbPosition.SelectedValue = _employee.PositionId;

                        dpHireDate.SelectedDate = _employee.HireDate;
                        dpFireDate.SelectedDate = _employee.FireDate;

                        txtNotes.Text = _employee.Notes;

                        // Заполняем данные водительского удостоверения, если есть
                        txtDriverLicenseNumber.Text = _employee.DriverLicenseNumber;
                        dpDriverLicenseExpiration.SelectedDate = _employee.DriverLicenseExpiration;

                        // Обновляем видимость раздела для водителей
                        UpdateDriverInfoVisibility(_employee.PositionId);

                        // Загружаем данные учетной записи пользователя, если есть
                        if (_employee.UserId.HasValue)
                        {
                            _user = context.User.Find(_employee.UserId.Value);
                            if (_user != null)
                            {
                                chkCreateUser.IsChecked = true;
                                txtUserName.Text = _user.UserName;
                                txtEmail.Text = _user.Email;
                                cmbUserRole.SelectedValue = _user.UserRoleId;

                                // Пароль не выводим в целях безопасности
                                txtPassword.Password = "";
                            }
                        }
                        else
                        {
                            chkCreateUser.IsChecked = false;

                            // По умолчанию предлагаем создать пользователя с данными сотрудника
                            string suggestedUsername = (_employee.FirstName.Length > 0 ? _employee.FirstName.Substring(0, 1).ToLower() : "") +
                                                      _employee.LastName.ToLower();
                            txtUserName.Text = suggestedUsername;

                            if (!string.IsNullOrEmpty(_employee.Phone))
                                txtEmail.Text = suggestedUsername + "@aquavita.ru";
                        }

                        // Обновляем доступность полей учетной записи
                        UpdateUserAccountVisibility();
                    }
                    else
                    {
                        MessageBox.Show("Сотрудник не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных сотрудника: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Обновляет видимость раздела для водителей
        /// </summary>
        private void UpdateDriverInfoVisibility(int positionId)
        {
            // Отображаем раздел для водителей только для должности "Водитель" (id = 2)
            bool isDriver = (positionId == 2);
            panelDriverInfo.Visibility = isDriver ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Обновляет доступность полей учетной записи пользователя
        /// </summary>
        private void UpdateUserAccountVisibility()
        {
            bool createUser = chkCreateUser.IsChecked ?? false;

            txtUserName.IsEnabled = createUser;
            txtEmail.IsEnabled = createUser;
            txtPassword.IsEnabled = createUser;
            cmbUserRole.IsEnabled = createUser;

            // При редактировании, если у сотрудника нет учетной записи и флажок не установлен,
            // очищаем поля учетной записи
            if (!createUser && _isEditMode && (_employee == null || !_employee.UserId.HasValue))
            {
                txtUserName.Text = string.Empty;
                txtEmail.Text = string.Empty;
                txtPassword.Password = string.Empty;
                if (cmbUserRole.Items.Count > 0)
                    cmbUserRole.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Проверяет заполнение обязательных полей
        /// </summary>
        private bool ValidateForm()
        {
            // Проверка основных полей
            if (string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                ShowError("Необходимо указать фамилию");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                ShowError("Необходимо указать имя");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                ShowError("Необходимо указать телефон");
                return false;
            }

            if (cmbPosition.SelectedItem == null)
            {
                ShowError("Необходимо выбрать должность");
                return false;
            }

            if (dpHireDate.SelectedDate == null)
            {
                ShowError("Необходимо указать дату приема на работу");
                return false;
            }

            // Проверка даты увольнения
            if (dpFireDate.SelectedDate != null && dpFireDate.SelectedDate < dpHireDate.SelectedDate)
            {
                ShowError("Дата увольнения не может быть раньше даты приема на работу");
                return false;
            }

            // Проверка полей для водителей
            int positionId = (int)cmbPosition.SelectedValue;
            bool isDriver = (positionId == 2);

            if (isDriver)
            {
                if (string.IsNullOrWhiteSpace(txtDriverLicenseNumber.Text))
                {
                    ShowError("Необходимо указать номер водительского удостоверения");
                    return false;
                }

                if (dpDriverLicenseExpiration.SelectedDate == null)
                {
                    ShowError("Необходимо указать срок действия водительского удостоверения");
                    return false;
                }

                if (dpDriverLicenseExpiration.SelectedDate <= DateTime.Today)
                {
                    ShowError("Срок действия водительского удостоверения должен быть в будущем");
                    return false;
                }
            }

            // Проверка полей учетной записи пользователя
            bool createUser = chkCreateUser.IsChecked ?? false;

            if (createUser)
            {
                if (string.IsNullOrWhiteSpace(txtUserName.Text))
                {
                    ShowError("Необходимо указать имя пользователя");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(txtEmail.Text))
                {
                    ShowError("Необходимо указать email");
                    return false;
                }

                if (!_isEditMode && string.IsNullOrWhiteSpace(txtPassword.Password))
                {
                    ShowError("Необходимо указать пароль");
                    return false;
                }

                if (cmbUserRole.SelectedItem == null)
                {
                    ShowError("Необходимо выбрать роль пользователя");
                    return false;
                }

                // Проверка уникальности имени пользователя и email
                using (var context = new WaterDeliveryEntities())
                {
                    // Проверка имени пользователя
                    if (!_isEditMode || (_isEditMode && _user != null && _user.UserName != txtUserName.Text.Trim()))
                    {
                        if (context.User.Any(u => u.UserName == txtUserName.Text.Trim()))
                        {
                            ShowError("Пользователь с таким именем уже существует");
                            return false;
                        }
                    }

                    // Проверка email
                    if (!_isEditMode || (_isEditMode && _user != null && _user.Email != txtEmail.Text.Trim()))
                    {
                        if (context.User.Any(u => u.Email == txtEmail.Text.Trim()))
                        {
                            ShowError("Пользователь с таким email уже существует");
                            return false;
                        }
                    }
                }

                // Валидация email
                if (!IsValidEmail(txtEmail.Text))
                {
                    ShowError("Указан некорректный email");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Проверяет корректность email
        /// </summary>
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
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
        /// Создает или обновляет учетную запись пользователя
        /// </summary>
        private int? SaveUser(WaterDeliveryEntities context)
        {
            bool createUser = chkCreateUser.IsChecked ?? false;
            if (!createUser)
                return null;

            User user;
            if (_isEditMode && _employee.UserId.HasValue)
            {
                // Обновляем существующего пользователя
                user = context.User.Find(_employee.UserId.Value);
                if (user == null)
                {
                    // Если пользователь не найден, создаем нового
                    user = new User();
                    context.User.Add(user);
                }
            }
            else
            {
                // Создаем нового пользователя
                user = new User
                {
                    RegistrationDate = DateTime.Now
                };
                context.User.Add(user);
            }

            // Заполняем данные пользователя
            user.UserName = txtUserName.Text.Trim();
            user.Email = txtEmail.Text.Trim();
            user.UserRoleId = (int)cmbUserRole.SelectedValue;
            user.Phone = txtPhone.Text.Trim();

            // Устанавливаем пароль, если он указан
            if (!string.IsNullOrWhiteSpace(txtPassword.Password))
                user.Password = txtPassword.Password;

            // Обновляем дату последнего входа
            user.LastLoginDate = DateTime.Now;

            // Сохраняем изменения, только если редактируем
            if (_isEditMode)
                context.SaveChanges();

            return user.UserId;
        }

        /// <summary>
        /// Сохраняет данные сотрудника
        /// </summary>
        private void SaveEmployee()
        {
            try
            {
                using (var context = new WaterDeliveryEntities())
                {
                    Employee employee;

                    // Получаем существующего сотрудника или создаем нового
                    if (_isEditMode)
                    {
                        employee = context.Employee.Find(_employeeId);
                    }
                    else
                    {
                        employee = new Employee
                        {
                            CreatedBy = CurrentUser.UserId
                        };
                        context.Employee.Add(employee);
                    }

                    // Сохраняем пользователя и получаем его ID
                    int? userId = SaveUser(context);

                    // Заполняем данные сотрудника
                    employee.FirstName = txtFirstName.Text.Trim();
                    employee.MiddleName = txtMiddleName.Text.Trim();
                    employee.LastName = txtLastName.Text.Trim();
                    employee.PositionId = (int)cmbPosition.SelectedValue;
                    employee.Phone = txtPhone.Text.Trim();
                    employee.Address = txtAddress.Text.Trim();
                    employee.HireDate = dpHireDate.SelectedDate.Value;
                    employee.FireDate = dpFireDate.SelectedDate;
                    employee.Notes = txtNotes.Text.Trim();
                    employee.UserId = userId;

                    // Заполняем данные водительского удостоверения, если это водитель
                    int positionId = (int)cmbPosition.SelectedValue;
                    bool isDriver = (positionId == 2);

                    if (isDriver)
                    {
                        employee.DriverLicenseNumber = txtDriverLicenseNumber.Text.Trim();
                        employee.DriverLicenseExpiration = dpDriverLicenseExpiration.SelectedDate;
                    }
                    else
                    {
                        employee.DriverLicenseNumber = null;
                        employee.DriverLicenseExpiration = null;
                    }

                    // Сохраняем изменения
                    context.SaveChanges();

                    DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных сотрудника: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Обработчики событий

        /// <summary>
        /// Обработчик изменения должности
        /// </summary>
        private void CmbPosition_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbPosition.SelectedItem is EmployeePosition position)
            {
                UpdateDriverInfoVisibility(position.PositionId);
            }
        }

        /// <summary>
        /// Обработчик изменения состояния флажка создания учетной записи
        /// </summary>
        private void ChkCreateUser_Checked(object sender, RoutedEventArgs e)
        {
            UpdateUserAccountVisibility();
        }

        /// <summary>
        /// Обработчик изменения состояния флажка создания учетной записи
        /// </summary>
        private void ChkCreateUser_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateUserAccountVisibility();
        }

        /// <summary>
        /// Обработчик нажатия кнопки сохранения
        /// </summary>
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            HideError();

            if (ValidateForm())
            {
                SaveEmployee();
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