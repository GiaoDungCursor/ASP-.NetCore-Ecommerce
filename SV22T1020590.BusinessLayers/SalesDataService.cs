using SV22T1020590.DataLayers.Interfaces;
using SV22T1020590.DataLayers.SQLServer;
using SV22T1020590.Models.Common;
using SV22T1020590.Models.Sales;
using SV22T1020590.BusinessLayers;

namespace SV22T1020590.BusinessLayers
{
    /// <summary>
    /// Cung cấp các chức năng xử lý dữ liệu liên quan đến bán hàng
    /// bao gồm: đơn hàng (Order) và chi tiết đơn hàng (OrderDetail).
    /// </summary>
    public static class SalesDataService
    {
        private static readonly IOrderRepository orderDB;

        private static string StatusCode(OrderStatusEnum status) => ((int)status).ToString();

        /// <summary>
        /// Constructor
        /// </summary>
        static SalesDataService()
        {
            orderDB = new OrderRepository(Configuration.ConnectionString);
        }

        #region Order

        /// <summary>
        /// Tìm kiếm và lấy danh sách đơn hàng dưới dạng phân trang.
        /// </summary>
        /// <param name="input">
        /// Thông tin tìm kiếm và phân trang đơn hàng.
        /// </param>
        /// <returns>
        /// Kết quả tìm kiếm dưới dạng danh sách đơn hàng có phân trang.
        /// </returns>
        public static async Task<PagedResult<OrderViewInfo>> ListOrdersAsync(OrderSearchInput input)
        {
            return await orderDB.ListAsync(input);
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một đơn hàng.
        /// </summary>
        /// <param name="orderID">Mã đơn hàng cần tìm.</param>
        /// <returns>
        /// Đối tượng OrderViewInfo nếu tìm thấy, ngược lại trả về null.
        /// </returns>
        public static async Task<OrderViewInfo?> GetOrderAsync(int orderID)
        {
            return await orderDB.GetAsync(orderID);
        }

        /// <summary>
        /// Tạo đơn hàng mới.
        /// </summary>
        /// <param name="data">Thông tin đơn hàng cần tạo.</param>
        /// <returns>Mã đơn hàng được tạo mới.</returns>
        public static async Task<int> AddOrderAsync(Order data)
        {
            data.Status = StatusCode(OrderStatusEnum.New);
            data.OrderTime = DateTime.Now;

            return await orderDB.AddAsync(data);
        }

        /// <summary>
        /// Cập nhật thông tin đơn hàng.
        /// </summary>
        /// <param name="data">Thông tin đơn hàng cần cập nhật.</param>
        /// <returns>
        /// True nếu cập nhật thành công, ngược lại False.
        /// </returns>
        public static async Task<bool> UpdateOrderAsync(Order data)
        {
            //TODO: Kiểm tra dữ liệu và trạng thái đơn hàng trước khi cập nhật
            return await orderDB.UpdateAsync(data);
        }

        /// <summary>
        /// Xóa đơn hàng.
        /// </summary>
        /// <param name="orderID">Mã đơn hàng cần xóa.</param>
        /// <returns>
        /// True nếu xóa thành công, ngược lại False.
        /// </returns>
        public static async Task<bool> DeleteOrderAsync(int orderID)
        {
            //TODO: Kiểm tra trạng thái đơn hàng trước khi xóa
            return await orderDB.DeleteAsync(orderID);
        }

        #endregion

        #region Order Status Processing

        /// <summary>
        /// Duyệt đơn hàng.
        /// </summary>
        /// <param name="orderID">Mã đơn hàng.</param>
        /// <param name="employeeID">Mã nhân viên duyệt.</param>
        /// <returns>
        /// True nếu duyệt thành công, ngược lại False.
        /// </returns>
        public static async Task<bool> AcceptOrderAsync(int orderID, int employeeID)
        {
            var order = await orderDB.GetAsync(orderID);
            if (order == null)
                return false;

            if (order.Status != StatusCode(OrderStatusEnum.New))
                return false;

            order.EmployeeID = employeeID;
            order.AcceptTime = DateTime.Now;
            order.Status = StatusCode(OrderStatusEnum.Accepted);

            return await orderDB.UpdateAsync(order);
        }

        /// <summary>
        /// Từ chối đơn hàng.
        /// </summary>
        /// <param name="orderID">Mã đơn hàng.</param>
        /// <param name="employeeID">Mã nhân viên từ chối.</param>
        /// <returns>
        /// True nếu từ chối thành công, ngược lại False.
        /// </returns>
        public static async Task<bool> RejectOrderAsync(int orderID, int employeeID)
        {
            var order = await orderDB.GetAsync(orderID);
            if (order == null)
                return false;

            if (order.Status != StatusCode(OrderStatusEnum.New))
                return false;

            order.EmployeeID = employeeID;
            order.FinishedTime = DateTime.Now;
            order.Status = StatusCode(OrderStatusEnum.Rejected);

            return await orderDB.UpdateAsync(order);
        }

        /// <summary>
        /// Hủy đơn hàng.
        /// </summary>
        /// <param name="orderID">Mã đơn hàng cần hủy.</param>
        /// <returns>
        /// True nếu hủy thành công, ngược lại False.
        /// </returns>
        public static async Task<bool> CancelOrderAsync(int orderID)
        {
            var order = await orderDB.GetAsync(orderID);
            if (order == null)
                return false;

            if (order.Status != StatusCode(OrderStatusEnum.New) &&
                order.Status != StatusCode(OrderStatusEnum.Accepted))
                return false;

            order.FinishedTime = DateTime.Now;
            order.Status = StatusCode(OrderStatusEnum.Cancelled);

            return await orderDB.UpdateAsync(order);
        }

        /// <summary>
        /// Giao đơn hàng cho người giao hàng.
        /// </summary>
        /// <param name="orderID">Mã đơn hàng.</param>
        /// <param name="shipperID">Mã người giao hàng.</param>
        /// <returns>
        /// True nếu giao thành công, ngược lại False.
        /// </returns>
        public static async Task<bool> ShipOrderAsync(int orderID, int shipperID)
        {
            var order = await orderDB.GetAsync(orderID);
            if (order == null)
                return false;

            if (order.Status != StatusCode(OrderStatusEnum.Accepted))
                return false;

            order.ShipperID = shipperID;
            order.ShippedTime = DateTime.Now;
            order.Status = StatusCode(OrderStatusEnum.Shipping);

            return await orderDB.UpdateAsync(order);
        }

        /// <summary>
        /// Hoàn tất đơn hàng.
        /// </summary>
        /// <param name="orderID">Mã đơn hàng.</param>
        /// <returns>
        /// True nếu hoàn tất thành công, ngược lại False.
        /// </returns>
        public static async Task<bool> CompleteOrderAsync(int orderID)
        {
            var order = await orderDB.GetAsync(orderID);
            if (order == null)
                return false;

            if (order.Status != StatusCode(OrderStatusEnum.Shipping))
                return false;

            order.FinishedTime = DateTime.Now;
            order.Status = StatusCode(OrderStatusEnum.Completed);

            return await orderDB.UpdateAsync(order);
        }

        #endregion

        #region Order Detail

        /// <summary>
        /// Lấy danh sách mặt hàng của đơn hàng.
        /// </summary>
        /// <param name="orderID">Mã đơn hàng.</param>
        /// <returns>
        /// Danh sách chi tiết mặt hàng trong đơn hàng.
        /// </returns>
        public static async Task<List<OrderDetailViewInfo>> ListDetailsAsync(int orderID)
        {
            return await orderDB.ListDetailsAsync(orderID);
        }

        /// <summary>
        /// Lấy thông tin một mặt hàng trong đơn hàng.
        /// </summary>
        /// <param name="orderID">Mã đơn hàng.</param>
        /// <param name="productID">Mã mặt hàng.</param>
        /// <returns>
        /// Đối tượng OrderDetailViewInfo nếu tìm thấy, ngược lại trả về null.
        /// </returns>
        public static async Task<OrderDetailViewInfo?> GetDetailAsync(int orderID, int productID)
        {
            return await orderDB.GetDetailAsync(orderID, productID);
        }

        /// <summary>
        /// Thêm mặt hàng vào đơn hàng.
        /// </summary>
        /// <param name="data">Thông tin chi tiết mặt hàng cần thêm.</param>
        /// <returns>
        /// True nếu thêm thành công, ngược lại False.
        /// </returns>
        public static async Task<bool> AddDetailAsync(OrderDetail data)
        {
            //TODO: Kiểm tra dữ liệu và trạng thái đơn hàng trước khi thêm mặt hàng
            return await orderDB.AddDetailAsync(data);
        }

        /// <summary>
        /// Cập nhật mặt hàng trong đơn hàng.
        /// </summary>
        /// <param name="data">Thông tin chi tiết mặt hàng cần cập nhật.</param>
        /// <returns>
        /// True nếu cập nhật thành công, ngược lại False.
        /// </returns>
        public static async Task<bool> UpdateDetailAsync(OrderDetail data)
        {
            //TODO: Kiểm tra dữ liệu và trạng thái đơn hàng trước khi cập nhật mặt hàng
            return await orderDB.UpdateDetailAsync(data);
        }

        /// <summary>
        /// Xóa mặt hàng khỏi đơn hàng.
        /// </summary>
        /// <param name="orderID">Mã đơn hàng.</param>
        /// <param name="productID">Mã mặt hàng cần xóa.</param>
        /// <returns>
        /// True nếu xóa thành công, ngược lại False.
        /// </returns>
        public static async Task<bool> DeleteDetailAsync(int orderID, int productID)
        {
            //TODO: Kiểm tra trạng thái đơn hàng trước khi xóa mặt hàng
            return await orderDB.DeleteDetailAsync(orderID, productID);
        }

        #endregion
    }
}