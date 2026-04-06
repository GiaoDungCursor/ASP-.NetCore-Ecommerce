using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SV22T1020590.DataLayers;
using SV22T1020590.Admin.Models;
using SV22T1020590.Models.Sales;
using System.Diagnostics;

namespace SV22T1020590.Admin.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            var model = new HomeDashboardViewModel();
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            // Quick counters.
            ProductDAL.List(_configuration, out var productCount, page: 1, pageSize: 1);
            CustomerDAL.List(_configuration, out var customerCount, page: 1, pageSize: 1);
            model.TotalProducts = productCount;
            model.TotalCustomers = customerCount;

            using (var connection = DatabaseHelper.CreateConnection(_configuration))
            {
                connection.Open();

                // Revenue + orders today.
                const string todaySql = @"
SELECT
    ISNULL(SUM(od.Quantity * od.SalePrice), 0) AS RevenueToday,
    COUNT(DISTINCT o.OrderID) AS OrdersToday
FROM Orders o
LEFT JOIN OrderDetails od ON od.OrderID = o.OrderID
WHERE o.OrderTime >= @Today
  AND o.OrderTime < @Tomorrow
  AND ISNULL(o.Status, '') NOT IN ('-1', '-2');";
                using (var cmd = new SqlCommand(todaySql, connection))
                {
                    cmd.Parameters.AddWithValue("@Today", today);
                    cmd.Parameters.AddWithValue("@Tomorrow", tomorrow);
                    using var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        model.RevenueToday = reader["RevenueToday"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["RevenueToday"]);
                        model.OrdersToday = reader["OrdersToday"] == DBNull.Value ? 0 : Convert.ToInt32(reader["OrdersToday"]);
                    }
                }

                // Revenue by month (last 6 months).
                const string revenueSql = @"
SELECT
    YEAR(o.OrderTime) AS [Year],
    MONTH(o.OrderTime) AS [Month],
    SUM(od.Quantity * od.SalePrice) AS Revenue
FROM Orders o
INNER JOIN OrderDetails od ON od.OrderID = o.OrderID
WHERE o.OrderTime >= @FromDate
  AND ISNULL(o.Status, '') NOT IN ('-1', '-2')
GROUP BY YEAR(o.OrderTime), MONTH(o.OrderTime)
ORDER BY [Year], [Month];";
                using (var cmd = new SqlCommand(revenueSql, connection))
                {
                    cmd.Parameters.AddWithValue("@FromDate", new DateTime(today.Year, today.Month, 1).AddMonths(-5));
                    using var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        model.MonthlyRevenue.Add(new MonthlyRevenueItem
                        {
                            Year = Convert.ToInt32(reader["Year"]),
                            Month = Convert.ToInt32(reader["Month"]),
                            Revenue = reader["Revenue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["Revenue"])
                        });
                    }
                }

                // Top selling products.
                const string topProductSql = @"
SELECT TOP 5
    p.ProductID,
    p.ProductName,
    SUM(od.Quantity) AS TotalQty
FROM OrderDetails od
INNER JOIN Orders o ON o.OrderID = od.OrderID
INNER JOIN Products p ON p.ProductID = od.ProductID
WHERE ISNULL(o.Status, '') NOT IN ('-1', '-2')
GROUP BY p.ProductID, p.ProductName
ORDER BY TotalQty DESC, p.ProductName ASC;";
                using (var cmd = new SqlCommand(topProductSql, connection))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        model.TopProducts.Add(new TopProductItem
                        {
                            ProductID = Convert.ToInt32(reader["ProductID"]),
                            ProductName = reader["ProductName"]?.ToString() ?? "",
                            Quantity = reader["TotalQty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["TotalQty"])
                        });
                    }
                }
            }

            // Orders requiring processing.
            model.PendingOrders = OrderDAL.List(
                _configuration,
                out _,
                status: "",
                page: 1,
                pageSize: 8)
                .Where(o => o.Status == ((int)OrderStatusEnum.New).ToString()
                         || o.Status == ((int)OrderStatusEnum.Accepted).ToString()
                         || o.Status == ((int)OrderStatusEnum.Shipping).ToString())
                .ToList();

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
