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
    /// Логика взаимодействия для ProductDialog.xaml
    /// </summary>
    public partial class ProductDialog : Window
    {
        private int _productId = 0;
        private bool _isEditMode = false;
        private Product _product;

        /// <summary>
        /// Конструктор для создания нового продукта
        /// </summary>
        public ProductDialog()
        {
            InitializeComponent();

            _isEditMode = false;
            txtTitle.Text = "Новый продукт";

            LoadLookupData();
            SetDefaultValues();
        }

        /// <summary>
        /// Конструктор для редактирования существующего продукта
        /// </summary>
        public ProductDialog(int productId)
        {
            InitializeComponent();

            _productId = productId;
            _isEditMode = true;
            txtTitle.Text = "Редактирование продукта";

            LoadLookupData();
            LoadProductData();
        }

        /// <summary>
        /// Устанавливает значения по умолчанию для нового продукта
        /// </summary>
        private void SetDefaultValues()
        {
            // Устанавливаем значения по умолчанию для текстовых полей
            txtDefaultPrice.Text = "0.00";
            txtMinStockLevel.Text = "0";
            txtMaxStockLevel.Text = "0";
        }

        /// <summary>
        /// Загружает справочные данные (типы продуктов, типы воды, единицы измерения)
        /// </summary>
        private void LoadLookupData()
        {
            try
            {
                using (var context = new WaterDeliveryEntities())
                {
                    // Загрузка типов продуктов
                    var productTypes = context.ProductType.ToList();
                    cmbProductType.ItemsSource = productTypes;
                    cmbProductType.SelectedValuePath = "ProductTypeId";

                    // Загрузка типов воды
                    var waterTypes = context.WaterType.ToList();
                    cmbWaterType.ItemsSource = waterTypes;
                    cmbWaterType.SelectedValuePath = "WaterTypeId";

                    // Загрузка единиц измерения
                    var units = context.Unit.ToList();
                    cmbUnit.ItemsSource = units;
                    cmbUnit.SelectedValuePath = "UnitId";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке справочных данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Загружает данные продукта для редактирования
        /// </summary>
        private void LoadProductData()
        {
            try
            {
                using (var context = new WaterDeliveryEntities())
                {
                    // Загружаем данные продукта
                    _product = context.Product
                        .Include(p => p.ProductType)
                        .Include(p => p.WaterType)
                        .Include(p => p.Unit)
                        .FirstOrDefault(p => p.ProductId == _productId);

                    if (_product != null)
                    {
                        // Заполняем поля формы
                        txtArticle.Text = _product.Article;
                        txtProductName.Text = _product.ProductName;
                        txtDescription.Text = _product.Description;
                        txtDefaultPrice.Text = _product.DefaultPrice.ToString();

                        if (_product.Weight.HasValue)
                            txtWeight.Text = _product.Weight.Value.ToString();

                        if (_product.Volume.HasValue)
                            txtVolume.Text = _product.Volume.Value.ToString();

                        if (_product.ExpirationDate.HasValue)
                        {
                            // Calculate days between now and expiration date
                            int daysDifference = (_product.ExpirationDate.Value - DateTime.Now).Days;
                            if (daysDifference > 0)
                                txtExpirationDate.Text = daysDifference.ToString();
                        }

                        if (_product.MinStockLevel.HasValue)
                            txtMinStockLevel.Text = _product.MinStockLevel.Value.ToString();

                        if (_product.MaxStockLevel.HasValue)
                            txtMaxStockLevel.Text = _product.MaxStockLevel.Value.ToString();

                        // Выбираем значения в комбобоксах
                        cmbProductType.SelectedValue = _product.ProductTypeId;
                        cmbUnit.SelectedValue = _product.UnitId;

                        if (_product.WaterTypeId.HasValue)
                            cmbWaterType.SelectedValue = _product.WaterTypeId.Value;

                        // Обновляем видимость панели информации о воде
                        UpdateWaterInfoPanelVisibility(_product.ProductTypeId);
                    }
                    else
                    {
                        MessageBox.Show("Продукт не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных продукта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Обновляет видимость панели информации о воде в зависимости от типа продукта
        /// </summary>
        private void UpdateWaterInfoPanelVisibility(int productTypeId)
        {
            // Показываем панель информации о воде только для типов продуктов с водой
            // (Бутилированная вода - id 1, Вода в цистернах - id 2)
            bool isWaterProduct = (productTypeId == 1 || productTypeId == 2);
            panelWaterInfo.Visibility = isWaterProduct ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Проверяет заполнение обязательных полей
        /// </summary>
        private bool ValidateForm()
        {
            // Проверка основных полей
            if (string.IsNullOrWhiteSpace(txtArticle.Text))
            {
                ShowError("Необходимо указать артикул");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtProductName.Text))
            {
                ShowError("Необходимо указать название продукта");
                return false;
            }

            if (cmbProductType.SelectedItem == null)
            {
                ShowError("Необходимо выбрать тип продукта");
                return false;
            }

            if (cmbUnit.SelectedItem == null)
            {
                ShowError("Необходимо выбрать единицу измерения");
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtDefaultPrice.Text))
            {
                ShowError("Необходимо указать цену");
                return false;
            }

            // Проверка числовых значений
            if (!decimal.TryParse(txtDefaultPrice.Text, out decimal price) || price < 0)
            {
                ShowError("Цена должна быть неотрицательным числом");
                return false;
            }

            // Проверка полей для продуктов с водой
            int productTypeId = (int)cmbProductType.SelectedValue;
            bool isWaterProduct = (productTypeId == 1 || productTypeId == 2);

            if (isWaterProduct)
            {
                if (cmbWaterType.SelectedItem == null)
                {
                    ShowError("Необходимо выбрать тип воды");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(txtVolume.Text))
                {
                    ShowError("Необходимо указать объем");
                    return false;
                }

                if (!decimal.TryParse(txtVolume.Text, out decimal volume) || volume <= 0)
                {
                    ShowError("Объем должен быть положительным числом");
                    return false;
                }
            }

            // Проверка опциональных числовых полей
            if (!string.IsNullOrWhiteSpace(txtWeight.Text) &&
                (!decimal.TryParse(txtWeight.Text, out decimal weight) || weight <= 0))
            {
                ShowError("Вес должен быть положительным числом");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(txtExpirationDate.Text) &&
                (!int.TryParse(txtExpirationDate.Text, out int expDays) || expDays <= 0))
            {
                ShowError("Срок годности должен быть положительным числом дней");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(txtMinStockLevel.Text) &&
                (!decimal.TryParse(txtMinStockLevel.Text, out decimal minStock) || minStock < 0))
            {
                ShowError("Минимальный запас должен быть неотрицательным числом");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(txtMaxStockLevel.Text) &&
                (!decimal.TryParse(txtMaxStockLevel.Text, out decimal maxStock) || maxStock < 0))
            {
                ShowError("Максимальный запас должен быть неотрицательным числом");
                return false;
            }

            // Проверка, что максимальный запас больше или равен минимальному
            if (!string.IsNullOrWhiteSpace(txtMinStockLevel.Text) && !string.IsNullOrWhiteSpace(txtMaxStockLevel.Text))
            {
                decimal min = decimal.Parse(txtMinStockLevel.Text);
                decimal max = decimal.Parse(txtMaxStockLevel.Text);

                if (max < min)
                {
                    ShowError("Максимальный запас должен быть больше или равен минимальному");
                    return false;
                }
            }

            // Проверка уникальности артикула
            if (!_isEditMode || (_isEditMode && _product.Article != txtArticle.Text.Trim()))
            {
                using (var context = new WaterDeliveryEntities())
                {
                    if (context.Product.Any(p => p.Article == txtArticle.Text.Trim()))
                    {
                        ShowError("Продукт с таким артикулом уже существует");
                        return false;
                    }
                }
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
        /// Сохраняет данные продукта
        /// </summary>
        private void SaveProduct()
        {
            try
            {
                using (var context = new WaterDeliveryEntities())
                {
                    Product product;

                    // Получаем существующий продукт или создаем новый
                    if (_isEditMode)
                    {
                        product = context.Product.Find(_productId);
                    }
                    else
                    {
                        product = new Product
                        {
                            CreatedDate = DateTime.Now,
                            CreatedBy = CurrentUser.UserId
                        };
                        context.Product.Add(product);
                    }

                    // Заполняем данные продукта
                    product.Article = txtArticle.Text.Trim();
                    product.ProductName = txtProductName.Text.Trim();
                    product.ProductTypeId = (int)cmbProductType.SelectedValue;
                    product.Description = txtDescription.Text.Trim();
                    product.UnitId = (int)cmbUnit.SelectedValue;
                    product.DefaultPrice = decimal.Parse(txtDefaultPrice.Text);

                    // Заполняем опциональные поля
                    if (!string.IsNullOrWhiteSpace(txtWeight.Text))
                        product.Weight = decimal.Parse(txtWeight.Text);
                    else
                        product.Weight = null;

                    if (!string.IsNullOrWhiteSpace(txtMinStockLevel.Text))
                        product.MinStockLevel = decimal.Parse(txtMinStockLevel.Text);
                    else
                        product.MinStockLevel = null;

                    if (!string.IsNullOrWhiteSpace(txtMaxStockLevel.Text))
                        product.MaxStockLevel = decimal.Parse(txtMaxStockLevel.Text);
                    else
                        product.MaxStockLevel = null;

                    // Заполняем поля для продуктов с водой
                    int productTypeId = (int)cmbProductType.SelectedValue;
                    bool isWaterProduct = (productTypeId == 1 || productTypeId == 2);

                    if (isWaterProduct)
                    {
                        product.WaterTypeId = (int)cmbWaterType.SelectedValue;
                        product.Volume = decimal.Parse(txtVolume.Text);

                        if (!string.IsNullOrWhiteSpace(txtExpirationDate.Text) && int.TryParse(txtExpirationDate.Text, out int days))
                            product.ExpirationDate = DateTime.Now.AddDays(days);
                        else
                            product.ExpirationDate = null;
                    }
                    else
                    {
                        product.WaterTypeId = null;
                        product.Volume = null;
                        product.ExpirationDate = null;
                    }

                    // Сохраняем изменения
                    context.SaveChanges();

                    DialogResult = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных продукта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Обработчики событий

        /// <summary>
        /// Обработчик изменения типа продукта
        /// </summary>
        private void CmbProductType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbProductType.SelectedItem != null)
            {
                int productTypeId = (int)cmbProductType.SelectedValue;
                UpdateWaterInfoPanelVisibility(productTypeId);
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
                SaveProduct();
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