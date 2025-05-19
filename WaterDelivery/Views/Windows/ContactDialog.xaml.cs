using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WaterDelivery.Data;
using SchoolApp.Models;

namespace WaterDelivery.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для ContactDialog.xaml
    /// </summary>
    public partial class ContactDialog : Window
    {
        private int _clientId;
        private ClientContact _contact;
        private bool _isEditMode;

        /// <summary>
        /// Конструктор для создания нового контакта
        /// </summary>
        public ContactDialog(int clientId)
        {
            InitializeComponent();

            _clientId = clientId;
            _contact = null;
            _isEditMode = false;
            txtTitle.Text = "Новый контакт";
        }

        /// <summary>
        /// Конструктор для редактирования существующего контакта
        /// </summary>
        public ContactDialog(ClientContact contact)
        {
            InitializeComponent();

            _contact = contact;
            _clientId = contact.ClientId;
            _isEditMode = true;
            txtTitle.Text = "Редактирование контакта";

            // Заполняем поля формы
            txtLastName.Text = contact.LastName;
            txtFirstName.Text = contact.FirstName;
            txtMiddleName.Text = contact.MiddleName;
            txtPhone.Text = contact.Phone;
            txtEmail.Text = contact.Email;
            txtNotes.Text = contact.Notes;
            chkIsMain.IsChecked = contact.IsMain;
        }

        /// <summary>
        /// Проверяет заполнение обязательных полей
        /// </summary>
        private bool ValidateForm()
        {
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

            // Если указан email, проверяем его корректность
            if (!string.IsNullOrWhiteSpace(txtEmail.Text) && !IsValidEmail(txtEmail.Text))
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
        /// Сохраняет данные контакта
        /// </summary>
        private void SaveContact()
        {
            try
            {
                using (var context = new WaterDeliveryEntities())
                {
                    ClientContact contact;

                    // Получаем существующий контакт или создаем новый
                    if (_isEditMode)
                    {
                        contact = context.ClientContact.Find(_contact.ContactId);
                    }
                    else
                    {
                        contact = new ClientContact
                        {
                            ClientId = _clientId,
                            CreatedBy = CurrentUser.UserId
                        };
                        context.ClientContact.Add(contact);
                    }

                    // Заполняем данные контакта
                    contact.FirstName = txtFirstName.Text.Trim();
                    contact.MiddleName = txtMiddleName.Text.Trim();
                    contact.LastName = txtLastName.Text.Trim();
                    contact.Email = txtEmail.Text.Trim();
                    contact.Phone = txtPhone.Text.Trim();
                    contact.Notes = txtNotes.Text.Trim();

                    // Устанавливаем флаг основного контакта
                    bool isMain = chkIsMain.IsChecked ?? false;

                    // Если это основной контакт, снимаем флаг с других контактов клиента
                    if (isMain)
                    {
                        var otherContacts = context.ClientContact
                            .Where(c => c.ClientId == _clientId && c.ContactId != (_isEditMode ? _contact.ContactId : 0))
                            .ToList();

                        foreach (var otherContact in otherContacts)
                        {
                            otherContact.IsMain = false;
                        }
                    }

                    contact.IsMain = isMain;

                    // Сохраняем изменения
                    context.SaveChanges();

                    DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных контакта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Обработчики событий

        /// <summary>
        /// Обработчик нажатия кнопки сохранения
        /// </summary>
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            HideError();

            if (ValidateForm())
            {
                SaveContact();
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