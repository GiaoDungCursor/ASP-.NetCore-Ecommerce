namespace SV22T1020590.BusinessLayers
{
    /// <summary>
    /// Khởi tạo cho bussiness layer, cung cấp các tiện ích liên quan đến cấu hình của business layer
    /// </summary>
    public static class Configuration
    {
        private static string? _connectionString;
        /// <summary>
        /// khởi tạo cấu hình cho business layer, 
        /// cần được gọi trong Program.cs để cung cấp chuỗi kết nối đến CSDL SQL Server cho các lớp cài đặt xử lí dữ liệu
        /// </summary>
        /// <param name="connectionString"></param>
        public static void Initialize(string connectionString)
        {
            _connectionString = connectionString;
        }
        /// <summary>
        /// chuỗi tham số kết nối đến CSDL SQL Server, được cung cấp trong hàm Initialize
        /// </summary>
        public static string ConnectionString => _connectionString;
    }
}
