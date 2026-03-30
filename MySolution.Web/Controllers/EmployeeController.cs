using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using MySolution.Web.Models;
using System.Collections.Generic;

namespace MySolution.Web.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IConfiguration _configuration;

        public EmployeeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult List()
        {
            var employees = new List<Employee>();

            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return View("List", employees);
            }

            try
            {
                using (var connection = new SqlConnection(connectionString))
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
SELECT DeployID,
       first_name AS FirstName,
       last_name AS LastName,
       email     AS Email,
       phone     AS Phone,
       Department
FROM Employee";

                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var emp = new Employee
                            {
                                DeployID = reader["DeployID"]?.ToString() ?? string.Empty,
                                FirstName = reader["FirstName"]?.ToString() ?? string.Empty,
                                LastName = reader["LastName"]?.ToString() ?? string.Empty,
                                Email = reader["Email"]?.ToString() ?? string.Empty,
                                Phone = reader["Phone"]?.ToString() ?? string.Empty,
                                Department = reader["Department"]?.ToString() ?? string.Empty
                            };

                            employees.Add(emp);
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                // Consider logging
                return View("List", employees);
            }

            return View("List", employees);
        }
    }
}
