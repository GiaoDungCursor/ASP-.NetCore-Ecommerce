using SV22T1020590.Models.Sales;

namespace SV22T1020590.Admin.Models
{
    public class HomeDashboardViewModel
    {
        public decimal RevenueToday { get; set; }
        public int OrdersToday { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalProducts { get; set; }
        public List<MonthlyRevenueItem> MonthlyRevenue { get; set; } = new();
        public List<TopProductItem> TopProducts { get; set; } = new();
        public List<Order> PendingOrders { get; set; } = new();
    }

    public class MonthlyRevenueItem
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Revenue { get; set; }
        public string Label => $"Tháng {Month}";
    }

    public class TopProductItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
