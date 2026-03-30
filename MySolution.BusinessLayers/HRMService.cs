using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySolution.DataLayers;
using MySolution.DomainModels;

namespace MySolution.BusinessLayers
{
    public static class HRMService
    {
        private static EmployeeDAL? _employeeDal;

        /// <summary>
        /// Initialize DALs used by the business layer. Must be called before using other methods.
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        public static void Initialize(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("connectionString is required", nameof(connectionString));

            _employeeDal = new EmployeeDAL(connectionString);
        }

        private static void EnsureInitialized()
        {
            if (_employeeDal is null)
                throw new InvalidOperationException("HRMService is not initialized. Call HRMService.Initialize(connectionString) first.");
        }

        public static Task<IEnumerable<Employee>> ListEmployeeAsync()
        {
            EnsureInitialized();
            return _employeeDal!.ListAsync();
        }

        public static Task<Employee?> GetEmployeeAsync(string employeeId)
        {
            EnsureInitialized();
            if (string.IsNullOrWhiteSpace(employeeId)) throw new ArgumentException("employeeId is required", nameof(employeeId));
            return _employeeDal!.GetAsync(employeeId);
        }

        public static Task<string> AddEmployeeAsync(Employee employee)
        {
            EnsureInitialized();
            if (employee is null) throw new ArgumentNullException(nameof(employee));
            return _employeeDal!.AddAsync(employee);
        }

        public static Task<bool> UpdateEmployeeAsync(Employee employee)
        {
            EnsureInitialized();
            if (employee is null) throw new ArgumentNullException(nameof(employee));
            if (string.IsNullOrWhiteSpace(employee.EmployeeId)) throw new ArgumentException("EmployeeId is required for update", nameof(employee.EmployeeId));
            return _employeeDal!.UpdateAsync(employee);
        }

        public static Task<bool> DeleteEmployeeAsync(string employeeId)
        {
            EnsureInitialized();
            if (string.IsNullOrWhiteSpace(employeeId)) throw new ArgumentException("employeeId is required", nameof(employeeId));
            return _employeeDal!.DeleteAsync(employeeId);
        }
    }
}
