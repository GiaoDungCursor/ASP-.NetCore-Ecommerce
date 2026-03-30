using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Dapper;
using MySolution.DomainModels;

namespace MySolution.DataLayers
{
    public class EmployeeDAL
    {
        private readonly string _connectionString;

        public EmployeeDAL(string connectionString)
        {
            _connectionString = connectionString ?? throw new System.ArgumentNullException(nameof(connectionString));
        }

        public async Task<IEnumerable<Employee>> ListAsync()
        {
            const string sql = @"
SELECT
    EmployeeId,
    FirstName,
    LastName,
    BirthDate,
    Address,
    Email,
    Phone,
    Department
FROM Employee";

            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            return await conn.QueryAsync<Employee>(sql);
        }

        public async Task<Employee?> GetAsync(string id)
        {
            const string sql = @"
SELECT
    EmployeeId,
    FirstName,
    LastName,
    BirthDate,
    Address,
    Email,
    Phone,
    Department
FROM Employee
WHERE EmployeeId = @Id";

            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            return await conn.QueryFirstOrDefaultAsync<Employee>(sql, new { Id = id });
        }

        public async Task<string> AddAsync(Employee employee)
        {
            if (employee == null) throw new System.ArgumentNullException(nameof(employee));

            if (string.IsNullOrWhiteSpace(employee.EmployeeId))
            {
                employee.EmployeeId = System.Guid.NewGuid().ToString();
            }

            const string sql = @"
INSERT INTO Employee (EmployeeId, FirstName, LastName, BirthDate, Address, Email, Phone, Department)
VALUES (@EmployeeId, @FirstName, @LastName, @BirthDate, @Address, @Email, @Phone, @Department);";

            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            await conn.ExecuteAsync(sql, new
            {
                employee.EmployeeId,
                employee.FirstName,
                employee.LastName,
                employee.BirthDate,
                employee.Address,
                employee.Email,
                employee.Phone,
                employee.Department
            });

            return employee.EmployeeId;
        }

        public async Task<bool> UpdateAsync(Employee employee)
        {
            if (employee == null) throw new System.ArgumentNullException(nameof(employee));

            const string sql = @"
UPDATE Employee
SET
    FirstName = @FirstName,
    LastName = @LastName,
    BirthDate = @BirthDate,
    Address = @Address,
    Email = @Email,
    Phone = @Phone,
    Department = @Department
WHERE EmployeeId = @EmployeeId";

            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var rows = await conn.ExecuteAsync(sql, new
            {
                employee.FirstName,
                employee.LastName,
                employee.BirthDate,
                employee.Address,
                employee.Email,
                employee.Phone,
                employee.Department,
                employee.EmployeeId
            });

            return rows > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            const string sql = "DELETE FROM Employee WHERE EmployeeId = @Id";

            await using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var rows = await conn.ExecuteAsync(sql, new { Id = id });
            return rows > 0;
        }
    }
}
