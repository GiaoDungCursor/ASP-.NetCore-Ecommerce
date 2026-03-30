namespace SV22T1020590.Models.Sales
{
    /// <summary>
    /// Đơn hàng
    /// </summary>
    public class Order
    {
        /// <summary>
        /// Mã đơn hàng
        /// </summary>
        public int OrderID { get; set; }

        /// <summary>
        /// Mã khách hàng
        /// </summary>
        public int? CustomerID { get; set; }

        /// <summary>
        /// Thời điểm đặt hàng
        /// </summary>
        public DateTime OrderTime { get; set; }

        /// <summary>
        /// Tỉnh/thành giao hàng
        /// </summary>
        public string? DeliveryProvince { get; set; }

        /// <summary>
        /// Địa chỉ giao hàng
        /// </summary>
        public string? DeliveryAddress { get; set; }

        /// <summary>
        /// Mã nhân viên xử lý đơn hàng
        /// </summary>
        public int? EmployeeID { get; set; }

        /// <summary>
        /// Thời điểm duyệt đơn hàng
        /// </summary>
        public DateTime? AcceptTime { get; set; }

        /// <summary>
        /// Mã người giao hàng
        /// </summary>
        public int? ShipperID { get; set; }

        /// <summary>
        /// Thời điểm người giao hàng nhận đơn
        /// </summary>
        public DateTime? ShippedTime { get; set; }

        /// <summary>
        /// Thời điểm kết thúc đơn hàng
        /// </summary>
        public DateTime? FinishedTime { get; set; }

        /// <summary>
        /// Trạng thái hiện tại của đơn hàng
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Tên khách hàng
        /// </summary>
        public string? CustomerName { get; set; }

        /// <summary>
        /// Số điện thoại khách hàng
        /// </summary>
        public string? CustomerPhone { get; set; }

        /// <summary>
        /// Email khách hàng
        /// </summary>
        public string? CustomerEmail { get; set; }

        /// <summary>
        /// Tên nhân viên phụ trách
        /// </summary>
        public string? EmployeeName { get; set; }

        /// <summary>
        /// Tên người giao
        /// </summary>
        public string? ShipperName { get; set; }

        /// <summary>
        /// Số điện thoại người giao
        /// </summary>
        public string? ShipperPhone { get; set; }

        /// <summary>
        /// Mô tả trạng thái
        /// </summary>
        public string? StatusDescription { get; set; }

        /// <summary>
        /// Tổng tiền đơn hàng
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Chi tiết dòng hàng
        /// </summary>
        public List<OrderDetail>? OrderDetails { get; set; }
    }
}
