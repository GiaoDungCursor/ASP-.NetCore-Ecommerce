namespace SV22T1020590.Models.HR
{
    /// <summary>
    /// Nhân viên
    /// </summary>
    public class Employee
    {
        /// <summary>
        /// Mã nhân viên
        /// </summary>
        public int EmployeeID { get; set; }
        /// <summary>
        /// Họ và tên
        /// </summary>
        public string FullName { get; set; } = string.Empty;
        /// <summary>
        /// Ngày sinh
        /// </summary>
        public DateTime? BirthDate { get; set; }
        /// <summary>
        /// Địa chỉ
        /// </summary>
        public string? Address { get; set; }
        /// <summary>
        /// Điện thoại
        /// </summary>
        public string? Phone { get; set; }
        /// <summary>
        /// Email
        /// </summary>
        public string Email { get; set; } = string.Empty;
        /// <summary>Mật khẩu (lưu trong DB — map từ bảng Employees).</summary>
        public string? Password { get; set; }
        /// <summary>Danh sách vai trò dạng chuỗi (cột RoleNames).</summary>
        public string? RoleNames { get; set; }
        /// <summary>
        /// Tên file ảnh (nếu có)
        /// </summary>
        public string? Photo { get; set; }
        /// <summary>
        /// Nhân viên đang làm việc hay không? (bool — checkbox asp-for không hỗ trợ bool?)
        /// </summary>
        public bool IsWorking { get; set; } = true;
    }
}
