namespace SV22T1020590.Models.Sales
{
    /// <summary>
    /// Thông tin của một đơn hàng khi xem chi tiết (DTO)
    /// </summary>
    public class OrderViewInfo : Order
    {
        /// <summary>
        /// Tên nhân viên phụ trách đơn hàng
        /// </summary>
        public new string EmployeeName { get; set; } = "";

        /// <summary>
        /// Tên khách hàng (ghi đè để giữ khởi tạo mặc định trong view/DTO).
        /// </summary>
        public new string CustomerName { get; set; } = "";
        /// <summary>
        /// Tên giao dịch của khách hàng
        /// </summary>
        public string CustomerContactName { get; set; } = "";
        /// <summary>
        /// Email của khách hàng
        /// </summary>
        public new string CustomerEmail { get; set; } = "";
        /// <summary>
        /// Điện thoại khách hàng
        /// </summary>
        public new string CustomerPhone { get; set; } = "";
        /// <summary>
        /// Địa chỉ của khách hàng
        /// </summary>
        public string CustomerAddress { get; set; } = "";

        /// <summary>
        /// Tên người giao hàng
        /// </summary>
        public new string ShipperName { get; set; } = "";
        /// <summary>
        /// Điện thoại người giao hàng
        /// </summary>
        public new string ShipperPhone { get; set; } = "";
    }
}
