using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using SV22T1020590.Models.Sales;
using SalesOrderDetail = SV22T1020590.Models.Sales.OrderDetail;

namespace SV22T1020590.DataLayers
{
    public static class OrderDAL
    {
        public static List<Order> List(
            IConfiguration configuration,
            out int rowCount,
            string searchValue = "",
            string status = "",
            DateTime? fromTime = null,
            DateTime? toTime = null,
            int page = 1,
            int pageSize = 25)
        {
            var data = new List<Order>();
            rowCount = 0;

            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                var where = new StringBuilder("WHERE 1=1 ");
                if (!string.IsNullOrWhiteSpace(searchValue))
                    where.Append("AND (c.CustomerName LIKE '%' + @SearchValue + '%' OR o.DeliveryAddress LIKE '%' + @SearchValue + '%') ");
                if (!string.IsNullOrWhiteSpace(status))
                    where.Append("AND o.Status = @Status ");
                if (fromTime.HasValue)
                    where.Append("AND o.OrderTime >= @FromTime ");
                if (toTime.HasValue)
                    where.Append("AND o.OrderTime < @ToTime ");

                var joins = @"
                    LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
                    LEFT JOIN Employees e ON o.EmployeeID = e.EmployeeID
                    LEFT JOIN Shippers s ON o.ShipperID = s.ShipperID
                    LEFT JOIN OrderStatus os ON o.Status = os.Status
                    OUTER APPLY (
                        SELECT SUM(od.Quantity * od.SalePrice) AS TotalAmount
                        FROM OrderDetails od
                        WHERE od.OrderID = o.OrderID
                    ) tot";

                var countSql = $"SELECT COUNT(*) FROM Orders o {joins} {where}";
                using (var cmd = new SqlCommand(countSql, connection))
                {
                    if (!string.IsNullOrWhiteSpace(searchValue))
                        cmd.Parameters.AddWithValue("@SearchValue", searchValue);
                    if (!string.IsNullOrWhiteSpace(status))
                        cmd.Parameters.AddWithValue("@Status", status);
                    if (fromTime.HasValue)
                        cmd.Parameters.AddWithValue("@FromTime", fromTime.Value);
                    if (toTime.HasValue)
                        cmd.Parameters.AddWithValue("@ToTime", toTime.Value);
                    rowCount = Convert.ToInt32(cmd.ExecuteScalar());
                }

                var sql = $@"
                    SELECT * FROM (
                        SELECT o.OrderID, o.CustomerID, c.CustomerName, c.Phone AS CustomerPhone, c.Email AS CustomerEmail, o.OrderTime, o.DeliveryProvince, o.DeliveryAddress, 
                               o.EmployeeID, e.FullName AS EmployeeName, o.AcceptTime, o.ShipperID, s.ShipperName, s.Phone AS ShipperPhone, o.ShippedTime, o.FinishedTime, o.Status, os.Description AS StatusDescription,
                               ISNULL(tot.TotalAmount, 0) AS TotalAmount,
                               ROW_NUMBER() OVER (ORDER BY o.OrderID DESC) AS RowNumber
                        FROM Orders o
                        {joins}
                        {where}
                    ) AS T
                    WHERE RowNumber BETWEEN (@Page - 1) * @PageSize + 1 AND @Page * @PageSize
                    ORDER BY OrderID DESC";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    if (!string.IsNullOrWhiteSpace(searchValue))
                        cmd.Parameters.AddWithValue("@SearchValue", searchValue);
                    if (!string.IsNullOrWhiteSpace(status))
                        cmd.Parameters.AddWithValue("@Status", status);
                    if (fromTime.HasValue)
                        cmd.Parameters.AddWithValue("@FromTime", fromTime.Value);
                    if (toTime.HasValue)
                        cmd.Parameters.AddWithValue("@ToTime", toTime.Value);
                    cmd.Parameters.AddWithValue("@Page", page);
                    cmd.Parameters.AddWithValue("@PageSize", pageSize);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            data.Add(new Order
                            {
                                OrderID = Convert.ToInt32(reader["OrderID"]),
                                CustomerID = reader["CustomerID"] as int? ?? (reader["CustomerID"] == DBNull.Value ? null : (int?)Convert.ToInt32(reader["CustomerID"])),
                                CustomerName = reader["CustomerName"] as string,
                                CustomerPhone = reader["CustomerPhone"] as string,
                                CustomerEmail = reader["CustomerEmail"] as string,
                                OrderTime = reader["OrderTime"] as DateTime? ?? default,
                                DeliveryProvince = reader["DeliveryProvince"] as string,
                                DeliveryAddress = reader["DeliveryAddress"] as string,
                                EmployeeID = reader["EmployeeID"] as int?,
                                EmployeeName = reader["EmployeeName"] as string,
                                AcceptTime = reader["AcceptTime"] as DateTime?,
                                ShipperID = reader["ShipperID"] as int?,
                                ShipperName = reader["ShipperName"] as string,
                                ShipperPhone = reader["ShipperPhone"] as string,
                                ShippedTime = reader["ShippedTime"] as DateTime?,
                                FinishedTime = reader["FinishedTime"] as DateTime?,
                                Status = reader["Status"] == DBNull.Value ? null : reader["Status"]?.ToString(),
                                StatusDescription = reader["StatusDescription"] as string,
                                TotalAmount = reader["TotalAmount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["TotalAmount"])
                            });
                        }
                    }
                }
            }

            return data;
        }

        /// <summary>Danh sách đơn hàng của một khách hàng (cửa hàng).</summary>
        public static List<Order> ListByCustomer(IConfiguration configuration, int customerId, out int rowCount,
            int page = 1, int pageSize = 25)
        {
            var data = new List<Order>();
            rowCount = 0;

            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                var joins = @"
                    LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
                    LEFT JOIN Shippers s ON o.ShipperID = s.ShipperID
                    LEFT JOIN OrderStatus os ON o.Status = os.Status";

                var where = "WHERE o.CustomerID = @CustomerID";

                var countSql = $"SELECT COUNT(*) FROM Orders o {joins} {where}";
                using (var cmd = new SqlCommand(countSql, connection))
                {
                    cmd.Parameters.AddWithValue("@CustomerID", customerId);
                    rowCount = Convert.ToInt32(cmd.ExecuteScalar());
                }

                var sql = $@"
                    SELECT * FROM (
                        SELECT o.OrderID, o.CustomerID, c.CustomerName, c.Phone AS CustomerPhone, c.Email AS CustomerEmail, o.OrderTime, o.DeliveryProvince, o.DeliveryAddress, 
                               o.EmployeeID, o.AcceptTime, o.ShipperID, s.ShipperName, s.Phone AS ShipperPhone, o.ShippedTime, o.FinishedTime, o.Status, os.Description AS StatusDescription,
                               ROW_NUMBER() OVER (ORDER BY o.OrderID DESC) AS RowNumber
                        FROM Orders o
                        {joins}
                        {where}
                    ) AS T
                    WHERE RowNumber BETWEEN (@Page - 1) * @PageSize + 1 AND @Page * @PageSize
                    ORDER BY OrderID DESC";

                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@CustomerID", customerId);
                    cmd.Parameters.AddWithValue("@Page", page);
                    cmd.Parameters.AddWithValue("@PageSize", pageSize);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            data.Add(new Order
                            {
                                OrderID = Convert.ToInt32(reader["OrderID"]),
                                CustomerID = reader["CustomerID"] as int? ?? (reader["CustomerID"] == DBNull.Value ? null : (int?)Convert.ToInt32(reader["CustomerID"])),
                                CustomerName = reader["CustomerName"] as string,
                                CustomerPhone = reader["CustomerPhone"] as string,
                                CustomerEmail = reader["CustomerEmail"] as string,
                                OrderTime = reader["OrderTime"] as DateTime? ?? default,
                                DeliveryProvince = reader["DeliveryProvince"] as string,
                                DeliveryAddress = reader["DeliveryAddress"] as string,
                                EmployeeID = reader["EmployeeID"] as int?,
                                AcceptTime = reader["AcceptTime"] as DateTime?,
                                ShipperID = reader["ShipperID"] as int?,
                                ShipperName = reader["ShipperName"] as string,
                                ShipperPhone = reader["ShipperPhone"] as string,
                                ShippedTime = reader["ShippedTime"] as DateTime?,
                                FinishedTime = reader["FinishedTime"] as DateTime?,
                                Status = reader["Status"] == DBNull.Value ? null : reader["Status"]?.ToString(),
                                StatusDescription = reader["StatusDescription"] as string
                            });
                        }
                    }
                }
            }

            return data;
        }

        public static Order? Get(IConfiguration configuration, int orderId)
        {
            Order? order = null;

            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();

                string sql = @"SELECT o.OrderID, o.CustomerID, c.CustomerName, c.Phone AS CustomerPhone, c.Email AS CustomerEmail, o.OrderTime, o.DeliveryProvince, o.DeliveryAddress, 
                                o.EmployeeID, o.AcceptTime, o.ShipperID, s.ShipperName, s.Phone AS ShipperPhone, o.ShippedTime, o.FinishedTime, o.Status, os.Description AS StatusDescription
                                FROM Orders o
                                LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
                                LEFT JOIN Shippers s ON o.ShipperID = s.ShipperID
                                LEFT JOIN OrderStatus os ON o.Status = os.Status
                                WHERE o.OrderID = @OrderID";
                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@OrderID", orderId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            order = new Order
                            {
                                OrderID = Convert.ToInt32(reader["OrderID"]),
                                CustomerID = reader["CustomerID"] as int? ?? (reader["CustomerID"] == DBNull.Value ? null : (int?)Convert.ToInt32(reader["CustomerID"])),
                                CustomerName = reader["CustomerName"] as string,
                                CustomerPhone = reader["CustomerPhone"] as string,
                                CustomerEmail = reader["CustomerEmail"] as string,
                                OrderTime = reader["OrderTime"] as DateTime? ?? default,
                                DeliveryProvince = reader["DeliveryProvince"] as string,
                                DeliveryAddress = reader["DeliveryAddress"] as string,
                                EmployeeID = reader["EmployeeID"] as int?,
                                AcceptTime = reader["AcceptTime"] as DateTime?,
                                ShipperID = reader["ShipperID"] as int?,
                                ShipperName = reader["ShipperName"] as string,
                                ShipperPhone = reader["ShipperPhone"] as string,
                                ShippedTime = reader["ShippedTime"] as DateTime?,
                                FinishedTime = reader["FinishedTime"] as DateTime?,
                                Status = reader["Status"] == DBNull.Value ? null : reader["Status"]?.ToString(),
                                StatusDescription = reader["StatusDescription"] as string
                            };
                        }
                    }
                }

                if (order != null)
                {
                    var details = new List<SalesOrderDetail>();
                    string sqlDetails = "SELECT * FROM OrderDetails WHERE OrderID = @OrderID";
                    using (var cmd = new SqlCommand(sqlDetails, connection))
                    {
                        cmd.Parameters.AddWithValue("@OrderID", orderId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                details.Add(new SalesOrderDetail
                                {
                                    ProductID = Convert.ToInt32(reader["ProductID"]),
                                    Quantity = Convert.ToInt32(reader["Quantity"]),
                                    SalePrice = Convert.ToDecimal(reader["SalePrice"])
                                });
                            }
                        }
                    }

                    order.OrderDetails = details;
                }
            }

            return order;
        }

        public static int Add(IConfiguration configuration, Order order)
        {
            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();
                using (var tran = connection.BeginTransaction())
                {
                    try
                    {
                        string sql = @"INSERT INTO Orders
                            (CustomerID, OrderTime, DeliveryProvince, DeliveryAddress, EmployeeID, AcceptTime, ShipperID, ShippedTime, FinishedTime, Status)
                            VALUES
                            (@CustomerID, @OrderTime, @DeliveryProvince, @DeliveryAddress, @EmployeeID, @AcceptTime, @ShipperID, @ShippedTime, @FinishedTime, @Status);
                            SELECT SCOPE_IDENTITY();";

                        using (var cmd = new SqlCommand(sql, connection, tran))
                        {
                            cmd.Parameters.AddWithValue("@CustomerID", order.CustomerID ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@OrderTime", order.OrderTime);
                            cmd.Parameters.AddWithValue("@DeliveryProvince", order.DeliveryProvince ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@DeliveryAddress", order.DeliveryAddress ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@EmployeeID", order.EmployeeID ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@AcceptTime", order.AcceptTime ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@ShipperID", order.ShipperID ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@ShippedTime", order.ShippedTime ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@FinishedTime", order.FinishedTime ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@Status", order.Status ?? (object)DBNull.Value);

                            var idObj = cmd.ExecuteScalar();
                            int newId = Convert.ToInt32(idObj);
                            if (order.OrderDetails != null && order.OrderDetails.Count > 0)
                            {
                                foreach (var d in order.OrderDetails)
                                {
                                    string insertDetail = @"INSERT INTO OrderDetails (OrderID, ProductID, Quantity, SalePrice)
                                        VALUES (@OrderID, @ProductID, @Quantity, @SalePrice)";
                                    using (var cmd2 = new SqlCommand(insertDetail, connection, tran))
                                    {
                                        cmd2.Parameters.AddWithValue("@OrderID", newId);
                                        cmd2.Parameters.AddWithValue("@ProductID", d.ProductID);
                                        cmd2.Parameters.AddWithValue("@Quantity", d.Quantity);
                                        cmd2.Parameters.AddWithValue("@SalePrice", d.SalePrice);
                                        cmd2.ExecuteNonQuery();
                                    }
                                }
                            }

                            tran.Commit();
                            return newId;
                        }
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        public static bool SetStatus(IConfiguration configuration, int orderId, string status, int? employeeId = null, int? shipperId = null)
        {
            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();
                string sql = @"
                    UPDATE Orders
                    SET Status = @Status,
                        EmployeeID = CASE WHEN @EmployeeID IS NULL THEN EmployeeID ELSE @EmployeeID END,
                        ShipperID = CASE WHEN @ShipperID IS NULL THEN ShipperID ELSE @ShipperID END,
                        -- Keep milestone timestamps once captured; do not clear when moving forward.
                        AcceptTime = CASE WHEN @Status = '2' THEN ISNULL(AcceptTime, GETDATE()) ELSE AcceptTime END,
                        ShippedTime = CASE WHEN @Status = '3' THEN ISNULL(ShippedTime, GETDATE()) ELSE ShippedTime END,
                        FinishedTime = CASE WHEN @Status = '4' THEN ISNULL(FinishedTime, GETDATE()) ELSE FinishedTime END
                    WHERE OrderID = @OrderID";
                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@Status", status ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EmployeeID", employeeId.HasValue ? employeeId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@ShipperID", shipperId.HasValue ? shipperId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@OrderID", orderId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public static bool Delete(IConfiguration configuration, int orderId)
        {
            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();
                using (var tran = connection.BeginTransaction())
                {
                    try
                    {
                        string delDetails = "DELETE FROM OrderDetails WHERE OrderID = @OrderID";
                        using (var cmd = new SqlCommand(delDetails, connection, tran))
                        {
                            cmd.Parameters.AddWithValue("@OrderID", orderId);
                            cmd.ExecuteNonQuery();
                        }

                        string delOrder = "DELETE FROM Orders WHERE OrderID = @OrderID";
                        using (var cmd = new SqlCommand(delOrder, connection, tran))
                        {
                            cmd.Parameters.AddWithValue("@OrderID", orderId);
                            var rows = cmd.ExecuteNonQuery();
                            tran.Commit();
                            return rows > 0;
                        }
                    }
                    catch
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }

        public static bool UpdateCartItem(IConfiguration configuration, int orderId, int productId, int quantity, decimal? salePrice = null)
        {
            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();
                string sel = "SELECT COUNT(*) FROM OrderDetails WHERE OrderID = @OrderID AND ProductID = @ProductID";
                using (var cmd = new SqlCommand(sel, connection))
                {
                    cmd.Parameters.AddWithValue("@OrderID", orderId);
                    cmd.Parameters.AddWithValue("@ProductID", productId);
                    var count = Convert.ToInt32(cmd.ExecuteScalar());
                    if (count == 0)
                    {
                        if (quantity <= 0) return false;
                        string ins = "INSERT INTO OrderDetails (OrderID, ProductID, Quantity, SalePrice) VALUES (@OrderID,@ProductID,@Quantity,@SalePrice)";
                        using (var cmd2 = new SqlCommand(ins, connection))
                        {
                            cmd2.Parameters.AddWithValue("@OrderID", orderId);
                            cmd2.Parameters.AddWithValue("@ProductID", productId);
                            cmd2.Parameters.AddWithValue("@Quantity", quantity);
                            cmd2.Parameters.AddWithValue("@SalePrice", salePrice ?? 0m);
                            return cmd2.ExecuteNonQuery() > 0;
                        }
                    }
                    else
                    {
                        if (quantity <= 0)
                        {
                            string del = "DELETE FROM OrderDetails WHERE OrderID = @OrderID AND ProductID = @ProductID";
                            using (var cmd2 = new SqlCommand(del, connection))
                            {
                                cmd2.Parameters.AddWithValue("@OrderID", orderId);
                                cmd2.Parameters.AddWithValue("@ProductID", productId);
                                return cmd2.ExecuteNonQuery() > 0;
                            }
                        }
                        else
                        {
                            string upd = @"
                                UPDATE OrderDetails
                                SET Quantity = @Quantity,
                                    SalePrice = CASE WHEN @SalePrice IS NULL OR @SalePrice <= 0 THEN SalePrice ELSE @SalePrice END
                                WHERE OrderID = @OrderID AND ProductID = @ProductID";
                            using (var cmd2 = new SqlCommand(upd, connection))
                            {
                                cmd2.Parameters.AddWithValue("@Quantity", quantity);
                                cmd2.Parameters.AddWithValue("@SalePrice", salePrice.HasValue ? salePrice.Value : DBNull.Value);
                                cmd2.Parameters.AddWithValue("@OrderID", orderId);
                                cmd2.Parameters.AddWithValue("@ProductID", productId);
                                return cmd2.ExecuteNonQuery() > 0;
                            }
                        }
                    }
                }
            }
        }

        public static bool RemoveCartItem(IConfiguration configuration, int orderId, int productId)
        {
            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();
                string del = "DELETE FROM OrderDetails WHERE OrderID = @OrderID AND ProductID = @ProductID";
                using (var cmd = new SqlCommand(del, connection))
                {
                    cmd.Parameters.AddWithValue("@OrderID", orderId);
                    cmd.Parameters.AddWithValue("@ProductID", productId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public static bool ClearCart(IConfiguration configuration, int orderId)
        {
            using (var connection = DatabaseHelper.CreateConnection(configuration))
            {
                connection.Open();
                string del = "DELETE FROM OrderDetails WHERE OrderID = @OrderID";
                using (var cmd = new SqlCommand(del, connection))
                {
                    cmd.Parameters.AddWithValue("@OrderID", orderId);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}

