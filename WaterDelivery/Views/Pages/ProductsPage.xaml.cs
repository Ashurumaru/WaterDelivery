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
    /// Логика взаимодействия для ProductsPage.xaml
    /// </summary>
    public partial class ProductsPage : Page
    {
        // Используем ObservableCollection для автоматического обновления UI
        private ObservableCollection<Product> _products;
        private List<Product> _allProducts; // Полный список продуктов до фильтрации

        public ProductsPage()
        {
            InitializeComponent();

            // Инициализация коллекций
            _products = new ObservableCollection<Product>();
            _allProducts = new List<Product>();

            // Привязка источника данных для DataGrid
            dgProducts.ItemsSource = _products;

            // Загрузка данных
            LoadProductData();

            // Загрузка справочников
            LoadReferences();
        }

        /// <summary>
        /// Загружает данные продуктов
        /// </summary>
        private async void LoadProductData()
        {
            try
            {
                await Task.Run(() =>
                {
                    using (var context = new WaterDeliveryEntities())
                    {
                        // Загружаем продукты с типами и другими связанными данными
                        _allProducts = context.Product
                            .Include(p => p.ProductType)
                            .Include(p => p.WaterType)
                            .Include(p => p.Unit)
                            .ToList();
                    }
                });

                // Обновляем UI
                UpdateProductsList(_allProducts);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке данных продуктов: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Обновляет список продуктов в UI
        /// </summary>
        private void UpdateProductsList(List<Product> products)
        {
            _products.Clear();
            foreach (var product in products)
            {
                _products.Add(product);
            }

            // Обновляем счетчик
            txtProductCount.Text = $"Всего: {_products.Count} продуктов";
        }

        /// <summary>
        /// Загружает справочники (типы продуктов, типы воды)
        /// </summary>
        private void LoadReferences()
        {
            try
            {
                using (var context = new WaterDeliveryEntities())
                {
                    // Загружаем типы продуктов
                    var productTypes = context.ProductType.ToList();

                    // Добавляем пункт "Все типы"
                    var allTypes = new ProductType { ProductTypeId = 0, TypeName = "Все типы" };
                    productTypes.Insert(0, allTypes);

                    cmbProductType.ItemsSource = productTypes;
                    cmbProductType.DisplayMemberPath = "TypeName";
                    cmbProductType.SelectedIndex = 0;

                    // Загружаем типы воды
                    var waterTypes = context.WaterType.ToList();

                    // Добавляем пункт "Все типы"
                    var allWaterTypes = new WaterType { WaterTypeId = 0, TypeName = "Все типы" };
                    waterTypes.Insert(0, allWaterTypes);

                    cmbWaterType.ItemsSource = waterTypes;
                    cmbWaterType.DisplayMemberPath = "TypeName";
                    cmbWaterType.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке справочников: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Применяет фильтры к списку продуктов
        /// </summary>
        private void ApplyFilters()
        {
            int selectedTypeId = 0;
            int selectedWaterTypeId = 0;
            string searchText = txtSearch.Text.ToLower().Trim();

            // Получаем выбранный тип продукта
            if (cmbProductType.SelectedItem is ProductType selectedType)
            {
                selectedTypeId = selectedType.ProductTypeId;
            }

            // Получаем выбранный тип воды
            if (cmbWaterType.SelectedItem is WaterType selectedWaterType)
            {
                selectedWaterTypeId = selectedWaterType.WaterTypeId;
            }

            // Фильтруем продукты
            var filteredProducts = _allProducts.AsEnumerable();

            // Фильтр по типу продукта
            if (selectedTypeId > 0)
            {
                filteredProducts = filteredProducts.Where(p => p.ProductTypeId == selectedTypeId);
            }

            // Фильтр по типу воды
            if (selectedWaterTypeId > 0)
            {
                filteredProducts = filteredProducts.Where(p => p.WaterTypeId.HasValue && p.WaterTypeId.Value == selectedWaterTypeId);
            }

            // Фильтр по строке поиска
            if (!string.IsNullOrEmpty(searchText))
            {
                filteredProducts = filteredProducts.Where(p =>
                    (p.Article != null && p.Article.ToLower().Contains(searchText)) ||
                    (p.ProductName != null && p.ProductName.ToLower().Contains(searchText)) ||
                    (p.Description != null && p.Description.ToLower().Contains(searchText))
                );
            }

            // Обновляем список
            UpdateProductsList(filteredProducts.ToList());
        }

        /// <summary>
        /// Добавляет новый продукт
        /// </summary>
        private void AddNewProduct()
        {
            var dialog = new ProductDialog();
            if (dialog.ShowDialog() == true)
            {
                // Перезагружаем данные
                LoadProductData();
            }
        }

        /// <summary>
        /// Редактирует выбранный продукт
        /// </summary>
        private void EditProduct(Product product)
        {
            if (product != null)
            {
                var dialog = new ProductDialog(product.ProductId);
                if (dialog.ShowDialog() == true)
                {
                    // Перезагружаем данные
                    LoadProductData();
                }
            }
        }

        /// <summary>
        /// Удаляет выбранный продукт
        /// </summary>
        private void DeleteProduct(Product product)
        {
            if (product == null) return;

            // Запрашиваем подтверждение
            var result = MessageBox.Show(
                $"Вы действительно хотите удалить продукт '{product.ProductName}'?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var context = new WaterDeliveryEntities())
                    {
                        // Проверяем, есть ли связанные позиции заказов
                        if (context.OrderItem.Any(oi => oi.ProductId == product.ProductId))
                        {
                            MessageBox.Show(
                                "Невозможно удалить продукт, так как он используется в заказах.",
                                "Ошибка удаления",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                            return;
                        }

                        // Проверяем, есть ли связанные записи на складе
                        if (context.StockProduct.Any(sp => sp.ProductId == product.ProductId))
                        {
                            MessageBox.Show(
                                "Невозможно удалить продукт, так как он имеет остатки на складе.",
                                "Ошибка удаления",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                            return;
                        }

                        // Проверяем, есть ли связанные записи о перемещениях
                        if (context.MovementProduct.Any(mp => mp.ProductId == product.ProductId))
                        {
                            MessageBox.Show(
                                "Невозможно удалить продукт, так как он используется в перемещениях товаров.",
                                "Ошибка удаления",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                            return;
                        }

                        // Получаем продукт из БД
                        var dbProduct = context.Product.Find(product.ProductId);
                        if (dbProduct != null)
                        {
                            // Удаляем продукт
                            context.Product.Remove(dbProduct);
                            context.SaveChanges();

                            // Обновляем список
                            _allProducts.Remove(product);
                            _products.Remove(product);
                            txtProductCount.Text = $"Всего: {_products.Count} продуктов";

                            MessageBox.Show(
                                "Продукт успешно удален.",
                                "Удаление продукта",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "Ошибка при удалении продукта: " + ex.Message,
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        #region Обработчики событий

        private void BtnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            AddNewProduct();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            // Сбрасываем фильтры
            cmbProductType.SelectedIndex = 0;
            cmbWaterType.SelectedIndex = 0;
            txtSearch.Clear();

            // Показываем все продукты
            UpdateProductsList(_allProducts);
        }

        private void CmbProductType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void CmbWaterType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Product product)
            {
                EditProduct(product);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Product product)
            {
                DeleteProduct(product);
            }
        }

        #endregion
    }
}