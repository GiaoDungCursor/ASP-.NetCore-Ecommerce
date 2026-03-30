using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020590.DataLayers.Interfaces;
using SV22T1020590.Models.Common;
using SV22T1020590.Models.Partner;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace SV22T1020590.DataLayers.SQLServer
{
    public class CustomerRepository : BaseRepository, ICustomerRepository
    {
        public CustomerRepository(string connectionString) : base(connectionString)
        {
        }

        public async Task<int> AddAsync(Customer data)
        {
            const string sql = @"
INSERT INTO Customers (CustomerName, ContactName, Province, Address, Phone, Email, IsLocked)
VALUES (@CustomerName, @ContactName, @Province, @Address, @Phone, @Email, @IsLocked);
SELECT CAST(SCOPE_IDENTITY() as int);";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var id = await cn.QuerySingleAsync<int>(sql, data);
            return id;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = "DELETE FROM Customers WHERE CustomerID = @id";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var affected = await cn.ExecuteAsync(sql, new { id });
            return affected > 0;
        }

        public async Task<Customer?> GetAsync(int id)
        {
            const string sql = "SELECT CustomerID, CustomerName, ContactName, Province, Address, Phone, Email, IsLocked FROM Customers WHERE CustomerID = @id";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var item = await cn.QueryFirstOrDefaultAsync<Customer>(sql, new { id });
            return item;
        }

        public async Task<PagedResult<Customer>> ListAsync(PaginationSearchInput input)
        {
            var whereClauses = new List<string>();
            var parameters = new DynamicParameters();

            if (!string.IsNullOrWhiteSpace(input.SearchValue))
            {
                whereClauses.Add("(CustomerName LIKE @q OR ContactName LIKE @q OR Email LIKE @q)");
                parameters.Add("q", "%" + input.SearchValue + "%");
            }

            var where = whereClauses.Count > 0 ? "WHERE " + string.Join(" AND ", whereClauses) : string.Empty;

            var sqlCount = $"SELECT COUNT(*) FROM Customers {where};";

            string sqlData;
            if (input.PageSize == 0)
            {
                sqlData = $"SELECT CustomerID, CustomerName, ContactName, Province, Address, Phone, Email, IsLocked FROM Customers {where} ORDER BY CustomerID";
            }
            else
            {
                sqlData = $"SELECT CustomerID, CustomerName, ContactName, Province, Address, Phone, Email, IsLocked FROM Customers {where} ORDER BY CustomerID OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
                parameters.Add("Offset", input.Offset);
                parameters.Add("PageSize", input.PageSize);
            }

            using var cn = GetConnection();
            await cn.OpenAsync();

            var rowCount = await cn.ExecuteScalarAsync<int>(sqlCount, parameters);
            var items = (await cn.QueryAsync<Customer>(sqlData, parameters)).ToList();

            var result = new PagedResult<Customer>
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = rowCount,
                DataItems = items
            };
            return result;
        }

        public async Task<bool> UpdateAsync(Customer data)
        {
            const string sql = @"
UPDATE Customers
SET CustomerName = @CustomerName,
    ContactName = @ContactName,
    Province = @Province,
    Address = @Address,
    Phone = @Phone,
    Email = @Email,
    IsLocked = @IsLocked
WHERE CustomerID = @CustomerID";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var affected = await cn.ExecuteAsync(sql, data);
            return affected > 0;
        }

        public async Task<bool> IsUsedAsync(int id)
        {
            // Simple check: if there are orders for this customer then it's used.
            const string sql = "SELECT CASE WHEN EXISTS(SELECT 1 FROM Orders WHERE CustomerID = @id) THEN 1 ELSE 0 END";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var used = await cn.ExecuteScalarAsync<int>(sql, new { id });
            return used == 1;
        }

        public async Task<bool> ChangePasswordAsync(int customerID, string newPassword)
        {
            const string sql = "UPDATE Customers SET Password = @newPassword WHERE CustomerID = @customerID";
            using var cn = GetConnection();
            await cn.OpenAsync();
            var affected = await cn.ExecuteAsync(sql, new { customerID, newPassword });
            return affected > 0;
        }

        public async Task<bool> ValidateEmailAsync(string email, int id = 0)
        {
            if (id == 0)
            {
                const string sql = "SELECT CASE WHEN EXISTS(SELECT 1 FROM Customers WHERE Email = @email) THEN 1 ELSE 0 END";
                using var cn = GetConnection();
                await cn.OpenAsync();
                var exists = await cn.ExecuteScalarAsync<int>(sql, new { email });
                return exists == 0; // valid if not exists
            }
            else
            {
                const string sql = "SELECT CASE WHEN EXISTS(SELECT 1 FROM Customers WHERE Email = @email AND CustomerID <> @id) THEN 1 ELSE 0 END";
                using var cn = GetConnection();
                await cn.OpenAsync();
                var exists = await cn.ExecuteScalarAsync<int>(sql, new { email, id });
                return exists == 0;
            }
        }
    }
}
