using System;
using System.Data.Entity;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WaterDelivery.Data;
using LiveCharts;
using LiveCharts.Wpf;
using System.Linq;

namespace WaterDelivery.Views.Pages
{
    /// <summary>
    /// Логика взаимодействия для DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage : Page
    {
        public DashboardPage()
        {
            InitializeComponent();
            LoadDashboardData();
            DataContext = this;

            // Устанавливаем заголовок, который будет отображаться в MainWindow
            Title = "Панель управления";
        }

        #region Свойства графиков

        // Свойства для графика заказов
        public SeriesCollection OrdersSeriesCollection { get; set; }
        public string[] OrdersLabels { get; set; }
        public Func<double, string> OrdersYFormatter { get; set; }

        // Свойства для графика клиентов по типам
        public SeriesCollection ClientTypeSeriesCollection { get; set; }
        public string[] ClientTypeLabels { get; set; }
        public Func<double, string> ClientTypeFormatter { get; set; }

        // Свойства для графика доставки воды
        public SeriesCollection WaterDeliverySeriesCollection { get; set; }
        public string[] WaterDeliveryLabels { get; set; }
        public Func<double, string> WaterDeliveryFormatter { get; set; }

        #endregion

        /// <summary>
        /// Загружает данные для панели управления
        /// </summary>
        private void LoadDashboardData()
        {
            try
            {
                using (var context = new WaterDeliveryEntities())
                {
                    LoadOrdersStatistics(context);
                    LoadClientsStatistics(context);
                    LoadWaterDeliveryStatistics(context);
                    LoadGeneralStatistics(context);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

                // Загружаем тестовые данные, если не удалось подключиться к БД
                LoadTestData();
            }
        }

        /// <summary>
        /// Загружает статистику заказов из базы данных
        /// </summary>
        private void LoadOrdersStatistics(WaterDeliveryEntities context)
        {
            // Получаем даты последних 7 дней
            var lastWeekDates = Enumerable.Range(0, 7)
                .Select(i => DateTime.Now.Date.AddDays(-i))
                .Reverse()
                .ToList();

            // Сначала получаем первую дату отдельно
            var firstDate = lastWeekDates.First();

            // Затем используем эту переменную в запросе
            var orders = context.Order
                .Where(o => o.OrderDate >= firstDate)
                .ToList();

            // Получаем количество заказов по дням
            var orderCounts = lastWeekDates
                .Select(date => new {
                    Date = date,
                    Count = orders.Count(o => o.OrderDate.Date == date.Date)
                })
                .ToList();

            // Получаем количество доставленных заказов по дням
            var deliveredCounts = lastWeekDates
                .Select(date => new {
                    Date = date,
                    Count = orders.Count(o => o.OrderDate.Date == date.Date && o.ActualDeliveryDate != null)
                })
                .ToList();

            // Создаем коллекцию для графика
            OrdersSeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Заказы",
                    Values = new ChartValues<double>(orderCounts.Select(o => (double)o.Count)),
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = 10,
                    LineSmoothness = 0.3,
                    Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3949AB")),
                    Fill = new SolidColorBrush(Color.FromArgb(50, 57, 73, 171))
                },
                new LineSeries
                {
                    Title = "Доставлено",
                    Values = new ChartValues<double>(deliveredCounts.Select(o => (double)o.Count)),
                    PointGeometry = DefaultGeometries.Square,
                    PointGeometrySize = 8,
                    LineSmoothness = 0.3,
                    Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#26A69A")),
                    Fill = new SolidColorBrush(Color.FromArgb(50, 38, 166, 154))
                }
            };

            // Метки для оси X (дни недели)
            OrdersLabels = lastWeekDates.Select(d => d.ToString("dd.MM")).ToArray();

            // Форматирование значений оси Y
            OrdersYFormatter = value => value.ToString("N0");
        }

        /// <summary>
        /// Загружает статистику клиентов из базы данных
        /// </summary>
        private void LoadClientsStatistics(WaterDeliveryEntities context)
        {
            // Получаем статистику клиентов по типам
            var clientsByType = context.Client
                .GroupBy(c => c.ClientTypeId)
                .Select(g => new
                {
                    ClientTypeId = g.Key,
                    Count = g.Count()
                })
                .ToList();

            // Загружаем названия типов клиентов
            var clientTypes = context.ClientType.ToDictionary(t => t.ClientTypeId, t => t.TypeName);

            // Создаем коллекцию для графика
            ClientTypeSeriesCollection = new SeriesCollection();

            // Цвета для графика
            var colors = new string[] { "#3949AB", "#26A69A", "#FFCA28", "#EF5350" };
            int colorIndex = 0;

            // Добавляем данные в коллекцию
            foreach (var item in clientsByType)
            {
                string typeName = clientTypes.ContainsKey(item.ClientTypeId)
                    ? clientTypes[item.ClientTypeId]
                    : $"Тип {item.ClientTypeId}";

                ClientTypeSeriesCollection.Add(new PieSeries
                {
                    Title = typeName,
                    Values = new ChartValues<double> { item.Count },
                    DataLabels = true,
                    LabelPoint = point => $"{point.Y} ({point.Participation:P1})",
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colors[colorIndex % colors.Length]))
                });

                colorIndex++;
            }
        }

        /// <summary>
        /// Загружает статистику доставки воды из базы данных
        /// </summary>
        private void LoadWaterDeliveryStatistics(WaterDeliveryEntities context)
        {
            // Получаем текущий месяц
            var currentDate = DateTime.Now;
            var firstDayOfMonth = new DateTime(currentDate.Year, currentDate.Month, 1);

            // Получаем количество недель в месяце
            var weeksInMonth = 5; // Стандартно используем 5 недель для графика

            // Получаем заказы за текущий месяц
            var orderItems = context.OrderItem
                .Where(oi => oi.Order.OrderDate >= firstDayOfMonth)
                .Where(oi => oi.ProductId != null) // Только товары (не услуги)
                .Select(oi => new
                {
                    OrderDate = oi.Order.OrderDate,
                    ProductId = oi.ProductId,
                    WaterTypeId = oi.Product.WaterTypeId,
                    Quantity = oi.Quantity,
                    WaterTypeName = oi.Product.WaterType.TypeName
                })
                .Where(oi => oi.WaterTypeId != null) // Только заказы с водой
                .ToList();

            // Группируем по неделям и типам воды
            var waterDeliveryByWeek = orderItems
                .GroupBy(oi => new
                {
                    Week = ((oi.OrderDate.Day - 1) / 7) + 1, // Номер недели в месяце (1-5)
                    WaterTypeId = oi.WaterTypeId,
                    WaterTypeName = oi.WaterTypeName
                })
                .Select(g => new
                {
                    g.Key.Week,
                    g.Key.WaterTypeId,
                    g.Key.WaterTypeName,
                    TotalQuantity = g.Sum(oi => oi.Quantity)
                })
                .OrderBy(g => g.Week)
                .ThenBy(g => g.WaterTypeId)
                .ToList();

            // Создаем структуру данных для графика
            var waterTypes = waterDeliveryByWeek
                .Select(w => w.WaterTypeName)
                .Distinct()
                .ToList();

            // Инициализируем массивы для данных
            var waterDeliveryData = new Dictionary<string, double[]>();
            foreach (var waterType in waterTypes)
            {
                waterDeliveryData[waterType] = new double[weeksInMonth];
            }

            // Заполняем данные
            foreach (var item in waterDeliveryByWeek)
            {
                if (item.Week <= weeksInMonth && item.WaterTypeName != null)
                {
                    waterDeliveryData[item.WaterTypeName][item.Week - 1] = (double)item.TotalQuantity;
                }
            }

            // Создаем коллекцию для графика
            WaterDeliverySeriesCollection = new SeriesCollection();

            // Цвета для графика
            var colors = new string[] { "#5C6BC0", "#26A69A", "#42A5F5", "#FFCA28" };
            int colorIndex = 0;

            // Добавляем серии в коллекцию
            foreach (var waterType in waterTypes)
            {
                WaterDeliverySeriesCollection.Add(new ColumnSeries
                {
                    Title = waterType,
                    Values = new ChartValues<double>(waterDeliveryData[waterType]),
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colors[colorIndex % colors.Length]))
                });
                colorIndex++;
            }

            // Метки для оси X (недели месяца)
            WaterDeliveryLabels = Enumerable.Range(1, weeksInMonth)
                .Select(i => $"Неделя {i}")
                .ToArray();

            // Форматирование значений оси Y
            WaterDeliveryFormatter = value => value.ToString("N0") + " л";
        }

        /// <summary>
        /// Загружает общую статистику из базы данных
        /// </summary>
        private void LoadGeneralStatistics(WaterDeliveryEntities context)
        {
            // Количество активных заказов
            var activeOrdersCount = context.Order
                .Count(o => o.StatusId == 1 || o.StatusId == 2 || o.StatusId == 3); // Новый, Подтвержден, В пути
            txtActiveOrders.Text = activeOrdersCount.ToString();

            // Количество клиентов
            var clientsCount = context.Client.Count();
            txtTotalClients.Text = clientsCount.ToString();

            // Количество транспорта в работе
            var activeVehiclesCount = context.Vehicle
                .Count(v => v.StatusId == 12); // В эксплуатации
            txtActiveVehicles.Text = activeVehiclesCount.ToString();

            // Доход за текущий месяц
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;
            var monthlyIncome = context.Payment
                .Where(p => p.PaymentDate.Month == currentMonth && p.PaymentDate.Year == currentYear)
                .Sum(p => (decimal?)p.Amount) ?? 0;
            txtMonthlyIncome.Text = $"{monthlyIncome:N0} ₽";

            // Загружаем данные последних заказов
            LoadRecentOrders(context);
        }

        /// <summary>
        /// Загружает данные о последних заказах из базы данных
        /// </summary>
        private void LoadRecentOrders(WaterDeliveryEntities context)
        {
            // Получаем последние 5 заказов
            var recentOrders = context.Order
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .Select(o => new
                {
                    OrderId = o.OrderId,
                    ClientName = o.Client.CompanyName ?? (o.Client.LastName + " " + o.Client.FirstName),
                    OrderDate = o.OrderDate,
                    StatusId = o.StatusId,
                    StatusName = o.Status.StatusName
                })
                .ToList();

            // Создаем список объектов для отображения
            var recentOrdersList = new List<RecentOrder>();

            // Словарь цветов для статусов
            var statusColors = new Dictionary<int, string>
            {
                { 1, "#FFCA28" }, // Новый - Желтый
                { 2, "#FFCA28" }, // Подтвержден - Желтый
                { 3, "#42A5F5" }, // В пути - Синий
                { 4, "#66BB6A" }, // Доставлен - Зеленый
                { 5, "#EF5350" }, // Отменен - Красный
                { 6, "#EF5350" }  // Возврат - Красный
            };

            // Заполняем список
            foreach (var order in recentOrders)
            {
                string statusColor = "#BDBDBD"; // Серый по умолчанию
                if (statusColors.ContainsKey(order.StatusId))
                {
                    statusColor = statusColors[order.StatusId];
                }

                recentOrdersList.Add(new RecentOrder
                {
                    OrderId = order.OrderId,
                    Client = order.ClientName,
                    Date = order.OrderDate,
                    Status = order.StatusName,
                    StatusColor = statusColor
                });
            }

            // Привязываем данные к списку
            lvRecentOrders.ItemsSource = recentOrdersList;
        }

        /// <summary>
        /// Загружает тестовые данные, если нет доступа к БД
        /// </summary>
        private void LoadTestData()
        {
            // Данные о заказах за последние 7 дней
            OrdersSeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Заказы",
                    Values = new ChartValues<double> { 12, 15, 9, 18, 22, 17, 20 },
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = 10,
                    LineSmoothness = 0.3,
                    Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3949AB")),
                    Fill = new SolidColorBrush(Color.FromArgb(50, 57, 73, 171))
                },
                new LineSeries
                {
                    Title = "Доставлено",
                    Values = new ChartValues<double> { 10, 14, 8, 16, 19, 15, 18 },
                    PointGeometry = DefaultGeometries.Square,
                    PointGeometrySize = 8,
                    LineSmoothness = 0.3,
                    Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#26A69A")),
                    Fill = new SolidColorBrush(Color.FromArgb(50, 38, 166, 154))
                }
            };

            // Метки для оси X (дни недели)
            OrdersLabels = new[] { "Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Вс" };

            // Форматирование значений оси Y
            OrdersYFormatter = value => value.ToString("N0");

            // Распределение клиентов по типам
            ClientTypeSeriesCollection = new SeriesCollection
            {
                new PieSeries
                {
                    Title = "Физические лица",
                    Values = new ChartValues<double> { 75 },
                    DataLabels = true,
                    LabelPoint = point => $"{point.Y} ({point.Participation:P1})",
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3949AB"))
                },
                new PieSeries
                {
                    Title = "Юридические лица",
                    Values = new ChartValues<double> { 35 },
                    DataLabels = true,
                    LabelPoint = point => $"{point.Y} ({point.Participation:P1})",
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#26A69A"))
                },
                new PieSeries
                {
                    Title = "ИП",
                    Values = new ChartValues<double> { 20 },
                    DataLabels = true,
                    LabelPoint = point => $"{point.Y} ({point.Participation:P1})",
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCA28"))
                },
                new PieSeries
                {
                    Title = "Гос. учреждения",
                    Values = new ChartValues<double> { 10 },
                    DataLabels = true,
                    LabelPoint = point => $"{point.Y} ({point.Participation:P1})",
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF5350"))
                }
            };

            // Статистика доставки воды по типам за месяц
            WaterDeliverySeriesCollection = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Питьевая",
                    Values = new ChartValues<double> { 750, 820, 930, 850, 790 },
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#5C6BC0"))
                },
                new ColumnSeries
                {
                    Title = "Минеральная",
                    Values = new ChartValues<double> { 320, 380, 420, 390, 350 },
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#26A69A"))
                },
                new ColumnSeries
                {
                    Title = "Артезианская",
                    Values = new ChartValues<double> { 480, 510, 590, 560, 520 },
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#42A5F5"))
                },
                new ColumnSeries
                {
                    Title = "Техническая",
                    Values = new ChartValues<double> { 1200, 1350, 1500, 1430, 1280 },
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFCA28"))
                }
            };

            // Метки для оси X (недели месяца)
            WaterDeliveryLabels = new[] { "Неделя 1", "Неделя 2", "Неделя 3", "Неделя 4", "Неделя 5" };

            // Форматирование значений оси Y
            WaterDeliveryFormatter = value => value.ToString("N0") + " л";

            // Количество активных заказов
            txtActiveOrders.Text = "18";

            // Количество клиентов
            txtTotalClients.Text = "140";

            // Количество транспорта в работе
            txtActiveVehicles.Text = "7";


            // Доход за текущий месяц
            txtMonthlyIncome.Text = "345 870 ₽";

            // Загружаем данные последних заказов
            var recentOrders = new List<RecentOrder>
            {
                new RecentOrder {
                    OrderId = 1529,
                    Client = "Иванов И.П.",
                    Date = DateTime.Now.AddHours(-2),
                    Status = "Доставлен",
                    StatusColor = "#66BB6A"
                },
                new RecentOrder {
                    OrderId = 1528,
                    Client = "ООО \"ВостокСтрой\"",
                    Date = DateTime.Now.AddHours(-3),
                    Status = "Доставлен",
                    StatusColor = "#66BB6A"
                },
                new RecentOrder {
                    OrderId = 1527,
                    Client = "Петрова М.С.",
                    Date = DateTime.Now.AddHours(-4),
                    Status = "В пути",
                    StatusColor = "#42A5F5"
                },
                new RecentOrder {
                    OrderId = 1526,
                    Client = "ООО \"БайкалТур\"",
                    Date = DateTime.Now.AddHours(-5),
                    Status = "Подтвержден",
                    StatusColor = "#FFCA28"
                },
                new RecentOrder {
                    OrderId = 1525,
                    Client = "Сидоров А.И.",
                    Date = DateTime.Now.AddHours(-6),
                    Status = "Отменен",
                    StatusColor = "#EF5350"
                }
            };

            // Привязываем данные к списку
            lvRecentOrders.ItemsSource = recentOrders;
        }

        /// <summary>
        /// Класс для отображения данных о последних заказах
        /// </summary>
        public class RecentOrder
        {
            public int OrderId { get; set; }
            public string Client { get; set; }
            public DateTime Date { get; set; }
            public string Status { get; set; }
            public string StatusColor { get; set; }

            // Форматированная дата для отображения
            public string FormattedDate => Date.ToString("dd.MM.yyyy HH:mm");
        }
    }
}