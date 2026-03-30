using SV22T1020590.Models.Partner;

namespace SV22T1020590.DataLayers.Interfaces
{
    /// <summary>
    /// Định nghĩa các phép xử lý dữ liệu trên Customer
    /// </summary>
    public interface ICustomerRepository : IGenericRepository<Customer>
    {
        /// <summary>
        /// Kiểm tra xem một địa chỉ email có hợp lệ hay không?
        /// </summary>
        /// <param name="email">Email cần kiểm tra</param>
        /// <param name="id">
        /// Nếu id = 0: Kiểm tra email của khách hàng mới.
        /// Nếu id <> 0: Kiểm tra email đối với khách hàng đã tồn tại
        /// </param>
        /// <returns></returns>
        Task<bool> ValidateEmailAsync(string email, int id = 0);
        /// <summary>
        /// Đổi mật khẩu cho khách hàng
        /// </summary>
        /// <param name="customerID"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        Task<bool> ChangePasswordAsync(int customerID, string newPassword);
    }
}
