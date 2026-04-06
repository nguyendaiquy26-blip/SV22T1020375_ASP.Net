using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020375.DataLayers.Interfaces;
using SV22T1020375.Models.Common;
using SV22T1020375.Models.Sales;

namespace SV22T1020375.DataLayers.SQLServer
{
    /// <summary>
    /// Cài đặt các chức năng xử lý dữ liệu cho đơn hàng
    /// </summary>
    public class OrderRepository : IOrderRepository
    {
        private readonly string _connectionString;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối CSDL</param>
        public OrderRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Tìm kiếm và lấy danh sách đơn hàng dưới dạng phân trang (Hoàn chỉnh)
        /// </summary>
        public async Task<PagedResult<OrderViewInfo>> ListAsync(OrderSearchInput input)
        {
            using var connection = new SqlConnection(_connectionString);

            // 1. Tiền xử lý dữ liệu truyền vào
            string keyword = "%" + (input.SearchValue ?? "") + "%";
            int status = input.Status;

            DateTime? fromTime = null;
            DateTime? toTime = null;

            if (!string.IsNullOrWhiteSpace(input.DateFrom))
            {
                if (DateTime.TryParseExact(input.DateFrom, "d/M/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime d))
                    fromTime = d;
                else if (DateTime.TryParse(input.DateFrom, out DateTime d2))
                    fromTime = d2;
            }

            if (!string.IsNullOrWhiteSpace(input.DateTo))
            {
                if (DateTime.TryParseExact(input.DateTo, "d/M/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime d))
                    toTime = d.AddDays(1).AddTicks(-1);
                else if (DateTime.TryParse(input.DateTo, out DateTime d2))
                    toTime = d2.AddDays(1).AddTicks(-1);
            }

            // Đóng gói các tham số
            var parameters = new
            {
                keyword = keyword,
                status = status,
                fromTime = fromTime,
                toTime = toTime,
                offset = (input.Page - 1) * input.PageSize,
                pagesize = input.PageSize
            };

            // 2. Viết câu lệnh đếm tổng số dòng
            string sqlCount = @"
                SELECT COUNT(*)
                FROM Orders o
                LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
                WHERE (@status = 0 OR o.Status = @status) 
                  AND (c.CustomerName LIKE @keyword OR o.DeliveryAddress LIKE @keyword)
                  AND (@fromTime IS NULL OR o.OrderTime >= @fromTime)
                  AND (@toTime IS NULL OR o.OrderTime <= @toTime)";

            int rowCount = await connection.ExecuteScalarAsync<int>(sqlCount, parameters);

            // 3. Viết câu lệnh lấy dữ liệu phân trang
            string sqlSelect = @"
                SELECT o.*, c.CustomerName
                FROM Orders o
                LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
                WHERE (@status = 0 OR o.Status = @status)
                  AND (c.CustomerName LIKE @keyword OR o.DeliveryAddress LIKE @keyword)
                  AND (@fromTime IS NULL OR o.OrderTime >= @fromTime)
                  AND (@toTime IS NULL OR o.OrderTime <= @toTime)
                ORDER BY o.OrderTime DESC
                OFFSET @offset ROWS
                FETCH NEXT @pagesize ROWS ONLY";

            var data = await connection.QueryAsync<OrderViewInfo>(sqlSelect, parameters);

            return new PagedResult<OrderViewInfo>()
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = rowCount,
                DataItems = data.ToList()
            };
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một đơn hàng
        /// </summary>
        public async Task<OrderViewInfo?> GetAsync(int orderID)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryFirstOrDefaultAsync<OrderViewInfo>(
                @"SELECT o.*, c.CustomerName
                  FROM Orders o
                  LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
                  WHERE o.OrderID = @orderID",
                new { orderID });
        }

        /// <summary>
        /// Bổ sung đơn hàng mới
        /// </summary>
        public async Task<int> AddAsync(Order data)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"INSERT INTO Orders
                           (CustomerID, OrderTime, DeliveryProvince, DeliveryAddress,
                            EmployeeID, AcceptTime, ShipperID, ShippedTime,
                            FinishedTime, Status)
                           VALUES
                           (@CustomerID, @OrderTime, @DeliveryProvince, @DeliveryAddress,
                            @EmployeeID, @AcceptTime, @ShipperID, @ShippedTime,
                            @FinishedTime, @Status);
                           SELECT CAST(SCOPE_IDENTITY() AS INT);";

            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        /// <summary>
        /// Cập nhật thông tin đơn hàng
        /// </summary>
        public async Task<bool> UpdateAsync(Order data)
        {
            using var connection = new SqlConnection(_connectionString);

            int rows = await connection.ExecuteAsync(
                @"UPDATE Orders
                  SET CustomerID = @CustomerID,
                      OrderTime = @OrderTime,
                      DeliveryProvince = @DeliveryProvince,
                      DeliveryAddress = @DeliveryAddress,
                      EmployeeID = @EmployeeID,
                      AcceptTime = @AcceptTime,
                      ShipperID = @ShipperID,
                      ShippedTime = @ShippedTime,
                      FinishedTime = @FinishedTime,
                      Status = @Status
                  WHERE OrderID = @OrderID", data);

            return rows > 0;
        }

        /// <summary>
        /// Xóa đơn hàng
        /// </summary>
        public async Task<bool> DeleteAsync(int orderID)
        {
            using var connection = new SqlConnection(_connectionString);

            int rows = await connection.ExecuteAsync(
                @"DELETE FROM Orders
                  WHERE OrderID = @orderID",
                new { orderID });

            return rows > 0;
        }

        /// <summary>
        /// Lấy danh sách mặt hàng trong đơn hàng
        /// </summary>
        public async Task<List<OrderDetailViewInfo>> ListDetailsAsync(int orderID)
        {
            using var connection = new SqlConnection(_connectionString);

            var data = await connection.QueryAsync<OrderDetailViewInfo>(
                @"SELECT d.*, p.ProductName
                  FROM OrderDetails d
                  JOIN Products p ON d.ProductID = p.ProductID
                  WHERE d.OrderID = @orderID",
                new { orderID });

            return data.ToList();
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một mặt hàng trong đơn hàng
        /// </summary>
        public async Task<OrderDetailViewInfo?> GetDetailAsync(int orderID, int productID)
        {
            using var connection = new SqlConnection(_connectionString);

            return await connection.QueryFirstOrDefaultAsync<OrderDetailViewInfo>(
                @"SELECT d.*, p.ProductName
                  FROM OrderDetails d
                  JOIN Products p ON d.ProductID = p.ProductID
                  WHERE d.OrderID = @orderID AND d.ProductID = @productID",
                new { orderID, productID });
        }

        /// <summary>
        /// Bổ sung mặt hàng vào đơn hàng
        /// </summary>
        public async Task<bool> AddDetailAsync(OrderDetail data)
        {
            using var connection = new SqlConnection(_connectionString);

            int rows = await connection.ExecuteAsync(
                @"INSERT INTO OrderDetails(OrderID, ProductID, Quantity, SalePrice)
                  VALUES(@OrderID, @ProductID, @Quantity, @SalePrice)", data);

            return rows > 0;
        }

        /// <summary>
        /// Cập nhật số lượng và giá bán của một mặt hàng trong đơn hàng
        /// </summary>
        public async Task<bool> UpdateDetailAsync(OrderDetail data)
        {
            using var connection = new SqlConnection(_connectionString);

            int rows = await connection.ExecuteAsync(
                @"UPDATE OrderDetails
                  SET Quantity = @Quantity,
                      SalePrice = @SalePrice
                  WHERE OrderID = @OrderID
                    AND ProductID = @ProductID", data);

            return rows > 0;
        }

        /// <summary>
        /// Xóa một mặt hàng khỏi đơn hàng
        /// </summary>
        public async Task<bool> DeleteDetailAsync(int orderID, int productID)
        {
            using var connection = new SqlConnection(_connectionString);

            int rows = await connection.ExecuteAsync(
                @"DELETE FROM OrderDetails
                  WHERE OrderID = @orderID
                    AND ProductID = @productID",
                new { orderID, productID });

            return rows > 0;
        }
    }
}