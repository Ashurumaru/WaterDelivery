using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;
using WaterDelivery.Data;

namespace WaterDelivery.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для LoyaltyPointsDialog.xaml
    /// </summary>
    public partial class LoyaltyPointsDialog : Window
    {
        private int _clientId;
        private int _currentPoints;
        private Client _client;

        /// <summary>
        /// Конструктор диалога управления бонусными баллами
        /// </summary>
        public LoyaltyPointsDialog(int clientId, int currentPoints)
        {
            InitializeComponent();

            _clientId = clientId;
            _currentPoints = currentPoints;

            // Загружаем данные клиента
            LoadClientData();

            // Устанавливаем текущее количество баллов
            txtCurrentPoints.Text = _currentPoints.ToString();
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
                    _client = context.Client.Find(_clientId);

                    if (_client != null)
                    {
                        // Отображаем имя клиента
                        if (!string.IsNullOrEmpty(_client.CompanyName))
                        {
                            txtClientName.Text = _client.CompanyName;
                        }
                        else
                        {
                            txtClientName.Text = $"{_client.LastName} {_client.FirstName} {_client.MiddleName}".Trim();
                        }
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
        /// Сохраняет изменения баллов
        /// </summary>
        private void SavePointsChange()
        {
            // Проверяем введенное количество баллов
            if (!int.TryParse(txtPointsAmount.Text, out int pointsAmount) || pointsAmount <= 0)
            {
                ShowError("Введите корректное количество баллов");
                return;
            }

            try
            {
                using (var context = new WaterDeliveryEntities())
                {
                    // Получаем клиента из БД
                    var client = context.Client.Find(_clientId);

                    if (client != null)
                    {
                        // Определяем тип операции
                        bool isCredit = rbCredit.IsChecked ?? false;

                        // Проверяем, достаточно ли баллов для списания
                        if (!isCredit && client.LoyaltyPoints < pointsAmount)
                        {
                            ShowError("Недостаточно баллов для списания");
                            return;
                        }

                        // Создаем запись о движении баллов
                        var movement = new LoyaltyPointMovement
                        {
                            ClientId = _clientId,
                            MovementTypeId = isCredit ? 1 : 2, // 1 - начисление, 2 - списание
                            PointsAmount = pointsAmount,
                            MovementDate = DateTime.Now,
                            Notes = txtNotes.Text.Trim()
                        };

                        context.LoyaltyPointMovement.Add(movement);

                        // Обновляем баланс клиента
                        if (isCredit)
                        {
                            client.LoyaltyPoints += pointsAmount;
                        }
                        else
                        {
                            client.LoyaltyPoints -= pointsAmount;
                        }

                        context.SaveChanges();

                        DialogResult = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении изменений: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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

        #region Обработчики событий

        /// <summary>
        /// Проверяет ввод только цифр
        /// </summary>
        private void TxtPointsAmount_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        /// <summary>
        /// Обработчик нажатия кнопки закрытия
        /// </summary>
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        /// <summary>
        /// Обработчик нажатия кнопки отмены
        /// </summary>
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        /// <summary>
        /// Обработчик нажатия кнопки сохранения
        /// </summary>
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            HideError();
            SavePointsChange();
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