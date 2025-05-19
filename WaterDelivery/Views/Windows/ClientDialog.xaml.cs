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
    /// Логика взаимодействия для ClientDialog.xaml
    /// </summary>
    public partial class ClientDialog : Window
    {
        private int _clientId = 0;
        private bool _isEditMode = false;
        private Client _client;

        /// <summary>
        /// Конструктор для создания нового клиента
        /// </summary>
        public ClientDialog()
        {
            InitializeComponent();

            _isEditMode = false;
            txtTitle.Text = "Новый клиент";
            txtLoyaltyPoints.Text = "0";

            LoadLookupData();
        }

        /// <summary>
        /// Конструктор для редактирования существующего клиента
        /// </summary>
        public ClientDialog(int clientId)
        {
            InitializeComponent();

            _clientId = clientId;
            _isEditMode = true;
            txtTitle.Text = "Редактирование клиента";

            LoadLookupData();
            LoadClientData();
        }

        /// <summary>
        /// Загружает справочные данные (типы клиентов, зоны доставки)
        /// </summary>
        private void LoadLookupData()
        {
            try
            {
                using (var context = new WaterDeliveryEntities())
                {
                    // Загрузка типов клиентов
                    var clientTypes = context.ClientType.ToList();
                    cmbClientType.ItemsSource = clientTypes;
                    cmbClientType.SelectedValuePath = "ClientTypeId";

                    // Загрузка зон доставки
                    var deliveryAreas = context.DeliveryArea.ToList();
                    cmbDeliveryArea.ItemsSource = deliveryAreas;
                    cmbDeliveryArea.SelectedValuePath = "DeliveryAreaId";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке справочных данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Загружает данные клиента для редактирования
        /// </summary>
        private void LoadClientData()
        {
            try
            {
                using (var context = new WaterDeliveryEntities())
                {
                    // Загружаем данные клиента
                    _client = context.Client
                        .Include(c => c.ClientType)
                        .Include(c => c.DeliveryArea)
                        .FirstOrDefault(c => c.ClientId == _clientId);

                    if (_client != null)
                    {
                        // Заполняем поля формы
                        cmbClientType.SelectedValue = _client.ClientTypeId;
                        cmbDeliveryArea.SelectedValue = _client.DeliveryAreaId;

                        txtFirstName.Text = _client.FirstName;
                        txtMiddleName.Text = _client.MiddleName;
                        txtLastName.Text = _client.LastName;
                        txtCompanyName.Text = _client.CompanyName;
                        txtEmail.Text = _client.Email;
                        txtPhone.Text = _client.Phone;
                        txtLegalAddress.Text = _client.LegalAddress;
                        txtINN.Text = _client.INN;
                        txtKPP.Text = _client.KPP;
                        txtOGRN.Text = _client.OGRN;
                        txtBankName.Text = _client.BankName;
                        txtBankBIC.Text = _client.BankBIC;
                        txtBankAccount.Text = _client.BankAccount;
                        txtCorrespondentAccount.Text = _client.CorrespondentAccount;
                        txtLoyaltyPoints.Text = _client.LoyaltyPoints.ToString();
                        txtNotes.Text = _client.Notes;

                        // Отображаем или скрываем панель с информацией о компании
                        UpdateUIBasedOnClientType(_client.ClientTypeId);
                    }
                    else
                    {
                        MessageBox.Show("Клиент не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных клиента: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Обновляет интерфейс в зависимости от выбранного типа клиента
        /// </summary>
        private void UpdateUIBasedOnClientType(int clientTypeId)
        {
            // Отображаем панель с информацией о компании для юр. лиц, ИП и гос. учреждений
            bool isCompany = clientTypeId != 1; // не физ. лицо
            panelCompany.Visibility = isCompany ? Visibility.Visible : Visibility.Collapsed;

            // Отображаем поле КПП только для юр. лиц и гос. учреждений
            bool needsKPP = clientTypeId == 2 || clientTypeId == 4; // юр. лицо или гос. учреждение
            panelKPP.Visibility = needsKPP ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Проверяет заполнение обязательных полей
        /// </summary>
        private bool ValidateForm()
        {
            // Проверка основных полей
            if (cmbClientType.SelectedItem == null)
            {
                ShowError("Необходимо выбрать тип клиента");
                return false;
            }

            if (cmbDeliveryArea.SelectedItem == null)
            {
                ShowError("Необходимо выбрать зону доставки");
                return false;
            }

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
                ShowError("Необходимо указать номер телефона");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                ShowError("Необходимо указать email");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtLegalAddress.Text))
            {
                ShowError("Необходимо указать юридический адрес");
                return false;
            }


            // Проверка дополнительных полей для юр. лиц, ИП и гос. учреждений
            int clientTypeId = (int)cmbClientType.SelectedValue;
            bool isCompany = clientTypeId != 1; // не физ. лицо

            if (isCompany && string.IsNullOrWhiteSpace(txtCompanyName.Text))
            {
                ShowError("Необходимо указать название компании");
                return false;
            }


            // Валидация email
            if (!IsValidEmail(txtEmail.Text))
            {
                ShowError("Указан некорректный email");
                return false;
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
        /// Сохраняет данные клиента
        /// </summary>
        private void SaveClient()
        {
            try
            {
                using (var context = new WaterDeliveryEntities())
                {
                    Client client;

                    // Получаем существующего клиента или создаем нового
                    if (_isEditMode)
                    {
                        client = context.Client.Find(_clientId);
                    }
                    else
                    {
                        client = new Client
                        {
                            RegistrationDate = DateTime.Now,
                            CreatedBy = CurrentUser.UserId
                        };
                        context.Client.Add(client);
                    }

                    // Заполняем данные клиента
                    client.ClientTypeId = (int)cmbClientType.SelectedValue;
                    client.DeliveryAreaId = (int)cmbDeliveryArea.SelectedValue;
                    client.FirstName = txtFirstName.Text.Trim();
                    client.MiddleName = txtMiddleName.Text.Trim();
                    client.LastName = txtLastName.Text.Trim();
                    client.CompanyName = txtCompanyName.Text.Trim();
                    client.Email = txtEmail.Text.Trim();
                    client.Phone = txtPhone.Text.Trim();
                    client.LegalAddress = txtLegalAddress.Text.Trim();
                    client.INN = txtINN.Text.Trim();
                    client.KPP = txtKPP.Text.Trim();
                    client.OGRN = txtOGRN.Text.Trim();
                    client.BankName = txtBankName.Text.Trim();
                    client.BankBIC = txtBankBIC.Text.Trim();
                    client.BankAccount = txtBankAccount.Text.Trim();
                    client.CorrespondentAccount = txtCorrespondentAccount.Text.Trim();
                    client.Notes = txtNotes.Text.Trim();

                    // Обрабатываем бонусные баллы
                    if (int.TryParse(txtLoyaltyPoints.Text, out int loyaltyPoints))
                    {
                        // Если баллы изменились и это редактирование, создаем запись о движении баллов
                        if (_isEditMode && client.LoyaltyPoints != loyaltyPoints)
                        {
                            int difference = loyaltyPoints - client.LoyaltyPoints;

                            // Создаем запись о движении баллов
                            var movement = new LoyaltyPointMovement
                            {
                                ClientId = client.ClientId,
                                MovementTypeId = difference > 0 ? 1 : 2, // 1 - начисление, 2 - списание
                                PointsAmount = Math.Abs(difference),
                                MovementDate = DateTime.Now,
                                Notes = "Корректировка баллов администратором"
                            };

                            context.LoyaltyPointMovement.Add(movement);
                        }

                        client.LoyaltyPoints = loyaltyPoints;
                    }

                    // Сохраняем изменения
                    context.SaveChanges();

                    DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных клиента: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Обработчики событий

        /// <summary>
        /// Обработчик изменения типа клиента
        /// </summary>
        private void CmbClientType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbClientType.SelectedItem is ClientType selectedType)
            {
                UpdateUIBasedOnClientType(selectedType.ClientTypeId);
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
                SaveClient();
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