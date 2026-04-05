using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020375.DataLayers.Interfaces;
using SV22T1020375.Models.Common;
using SV22T1020375.Models.Partner;

namespace SV22T1020375.DataLayers.SQLServer
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly string _connectionString;

        public CustomerRepository(string connectionString)
        {
            _connectionString = connectionString ?? "";
        }

        public async Task<int> AddAsync(Customer data)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"INSERT INTO Customers
                           (CustomerName, ContactName, Province, Address, Phone, Email, IsLocked)
                           VALUES
                           (@CustomerName, @ContactName, @Province, @Address, @Phone, @Email, @IsLocked);
                           SELECT CAST(SCOPE_IDENTITY() as int);";
            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        public async Task<bool> UpdateAsync(Customer data)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"UPDATE Customers
                           SET CustomerName = @CustomerName,
                               ContactName = @ContactName,
                               Province = @Province,
                               Address = @Address,
                               Phone = @Phone,
                               Email = @Email,
                               IsLocked = @IsLocked
                           WHERE CustomerID = @CustomerID";
            int rows = await connection.ExecuteAsync(sql, data);
            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"DELETE FROM Customers WHERE CustomerID = @id";
            int rows = await connection.ExecuteAsync(sql, new { id });
            return rows > 0;
        }

        public async Task<Customer?> GetAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"SELECT CustomerID, CustomerName, ContactName, Province,
                                  Address, Phone, Email, IsLocked
                           FROM Customers WHERE CustomerID = @id";
            return await connection.QueryFirstOrDefaultAsync<Customer>(sql, new { id });
        }

        public async Task<bool> IsUsedAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"SELECT COUNT(*) FROM Orders WHERE CustomerID = @id";
            int count = await connection.ExecuteScalarAsync<int>(sql, new { id });
            return count > 0;
        }

        public async Task<PagedResult<Customer>> ListAsync(PaginationSearchInput input)
        {
            using var connection = new SqlConnection(_connectionString);
            int offset = (input.Page - 1) * input.PageSize;

            string countSql = @"SELECT COUNT(*) FROM Customers WHERE CustomerName LIKE @search";
            string dataSql = @"SELECT CustomerID, CustomerName, ContactName,
                                      Province, Address, Phone, Email, IsLocked
                               FROM Customers
                               WHERE CustomerName LIKE @search
                               ORDER BY CustomerName
                               OFFSET @offset ROWS
                               FETCH NEXT @pagesize ROWS ONLY";

            var parameters = new
            {
                search = $"%{input.SearchValue ?? ""}%",
                offset,
                pagesize = input.PageSize
            };

            int count = await connection.ExecuteScalarAsync<int>(countSql, parameters);
            var data = await connection.QueryAsync<Customer>(dataSql, parameters);

            return new PagedResult<Customer>()
            {
                Page = input.Page,
                PageSize = input.PageSize,
                RowCount = count,
                DataItems = data.ToList()
            };
        }

        public async Task<bool> ValidateEmailAsync(string email, int id = 0)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql;
            if (id == 0)
                sql = @"SELECT COUNT(*) FROM Customers WHERE Email = @email";
            else
                sql = @"SELECT COUNT(*) FROM Customers WHERE Email = @email AND CustomerID <> @id";

            int count = await connection.ExecuteScalarAsync<int>(sql, new { email, id });
            return count == 0;
        }

        public async Task<int> RegisterAsync(Customer data, string password)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"INSERT INTO Customers
                           (CustomerName, ContactName, Province, Address, Phone, Email, Password, IsLocked)
                           VALUES
                           (@CustomerName, @ContactName, @Province, @Address, @Phone, @Email, @Password, 0);
                           SELECT CAST(SCOPE_IDENTITY() as int);";

            var parameters = new
            {
                CustomerName = data.CustomerName ?? "",
                ContactName = data.ContactName ?? "",
                Province = data.Province ?? "",
                Address = data.Address ?? "",
                Phone = data.Phone ?? "",
                Email = data.Email ?? "",
                Password = password ?? ""
            };

            return await connection.ExecuteScalarAsync<int>(sql, parameters);
        }
    }
}