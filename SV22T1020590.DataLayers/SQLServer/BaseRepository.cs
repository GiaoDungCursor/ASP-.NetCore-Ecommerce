using Microsoft.Data.SqlClient;

namespace SV22T1020590.DataLayers.SQLServer
{
    /// <summary>
    /// Lớp cơ cho các lớp cài đặt các phép xử lí dữ liệu
    /// Trên CSDL SQL Server
    /// </summary>
    public abstract class BaseRepository
    {
        private string _connectionString;

        /// <summary>
        /// Constructor của lớp cơ sở, nhận vào chuỗi kết nối đến CSDL SQL Server để các lớp kế thừa có thể sử dụng
        /// </summary>
        /// <param name="connectionString"></param>
        public BaseRepository(string connectionString)
        {
            _connectionString = connectionString;
        }
        protected SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
