using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020375.DataLayers.Interfaces;
using SV22T1020375.Models.Partner;
using SV22T1020375.Models.Security;
using System;
using System.Threading.Tasks;

namespace SV22T1020375.DataLayers.SQLServer
{
    public class UserAccountRepository : IUserAccountRepository
    {
        private readonly string _connectionString;

        public UserAccountRepository(string connectionString)
        {
            _connectionString = connectionString
                ?? throw new ArgumentNullException(nameof(connectionString), "ConnectionString không được để trống.");
        }

        public async Task<UserAccount?> AuthorizeAsync(string userName, string password)
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
                throw new InvalidOperationException("ConnectionString chưa được khởi tạo cho UserAccountRepository.");

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // SỬA LỖI: Dùng UNION ALL để quét cả 2 bảng. 
            // Nếu là Employee thì lấy Role thực tế, nếu là Customer thì tự động gán Role là 'Customer'
            var account = await connection.QueryFirstOrDefaultAsync<UserAccount>(
                @"
                -- 1. Quét trong bảng Nhân viên (Employees)
                SELECT 
                    CAST(EmployeeID AS NVARCHAR(50)) AS UserId,
                    Email AS UserName,
                    FullName AS DisplayName,
                    Email,
                    Photo,
                    RoleNames
                FROM Employees 
                WHERE Email = @userName 
                  AND Password = @password 
                  AND IsWorking = 1

                UNION ALL

                -- 2. Quét trong bảng Khách hàng (Customers)
                SELECT 
                    CAST(CustomerID AS NVARCHAR(50)) AS UserId,
                    Email AS UserName,
                    CustomerName AS DisplayName,
                    Email,
                    '' AS Photo,
                    'Customer' AS RoleNames
                FROM Customers 
                WHERE Email = @userName 
                  AND Password = @password 
                  AND IsLocked = 0
                ",
                new { userName, password });

            return account;
        }

        public async Task<bool> ChangePasswordAsync(string userName, string password)
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
                throw new InvalidOperationException("ConnectionString chưa được khởi tạo.");

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            int rows1 = await connection.ExecuteAsync(
                @"UPDATE Employees SET Password = @password WHERE Email = @userName",
                new { userName, password });

            int rows2 = await connection.ExecuteAsync(
                @"UPDATE Customers SET Password = @password WHERE Email = @userName",
                new { userName, password });

            return (rows1 + rows2) > 0;
        }

        public async Task<int> RegisterCustomerAsync(Customer data, string password)
        {
            int customerId = 0;
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                var sql = @"
                    IF EXISTS(SELECT * FROM Customers WHERE Email = @Email)
                    BEGIN
                        SELECT 0;
                    END
                    ELSE
                    BEGIN
                        INSERT INTO Customers (CustomerName, ContactName, Province, Address, Phone, Email, Password, IsLocked)
                        VALUES (@CustomerName, @ContactName, @Province, @Address, @Phone, @Email, @Password, 0);

                        SELECT CAST(SCOPE_IDENTITY() AS INT);
                    END";

                var parameters = new
                {
                    CustomerName = data.CustomerName ?? "",
                    ContactName = data.ContactName ?? "",
                    Province = data.Province ?? "",
                    Address = data.Address ?? "",
                    Phone = data.Phone ?? "",
                    Email = data.Email ?? "",
                    Password = password
                };

                customerId = await connection.ExecuteScalarAsync<int>(sql, parameters);
            }

            return customerId;
        }
    }
}