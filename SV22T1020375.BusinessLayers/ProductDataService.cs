using Microsoft.Data.SqlClient;
using SV22T1020375.Models;
using System;
using System.Collections.Generic;

namespace SV22T1020375.BusinessLayers
{
    public static class ProductDataService
    {
        public static string ConnectionString { get; set; } = @"Server=.\SQLEXPRESS;Database=LiteCommerceDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;";

        // 1. Hàm lấy danh sách sản phẩm (Đã thêm ĐIỀU KIỆN LỌC SQL)
        public static List<Product> ListOfProducts(out int rowCount, int page = 1, int pageSize = 0, string searchValue = "", int categoryID = 0, int supplierID = 0, decimal minPrice = 0, decimal maxPrice = 0)
        {
            rowCount = 0;
            List<Product> data = new List<Product>();

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                // TẠO CHUỖI ĐIỀU KIỆN LỌC CHUNG CHO CẢ COUNT VÀ SELECT
                string whereCondition = @"(IsSelling = 1) 
                                          AND (@SearchValue = N'' OR ProductName LIKE N'%' + @SearchValue + '%')
                                          AND (@CategoryID = 0 OR CategoryID = @CategoryID)
                                          AND (@SupplierID = 0 OR SupplierID = @SupplierID)
                                          AND (@MinPrice = 0 OR Price >= @MinPrice)
                                          AND (@MaxPrice = 0 OR Price <= @MaxPrice)";

                // Lấy tổng số dòng (đã có bộ lọc)
                string sqlCount = $"SELECT COUNT(*) FROM Products WHERE {whereCondition}";
                using (SqlCommand cmdCount = new SqlCommand(sqlCount, connection))
                {
                    cmdCount.Parameters.AddWithValue("@SearchValue", searchValue ?? "");
                    cmdCount.Parameters.AddWithValue("@CategoryID", categoryID);
                    cmdCount.Parameters.AddWithValue("@SupplierID", supplierID);
                    cmdCount.Parameters.AddWithValue("@MinPrice", minPrice);
                    cmdCount.Parameters.AddWithValue("@MaxPrice", maxPrice);

                    rowCount = Convert.ToInt32(cmdCount.ExecuteScalar());
                }

                // Lấy danh sách sản phẩm (đã có bộ lọc)
                string sql = $@"SELECT * FROM Products 
                                WHERE {whereCondition} 
                                ORDER BY ProductID DESC";

                if (pageSize > 0)
                {
                    sql += @" OFFSET (@Page - 1) * @PageSize ROWS 
                              FETCH NEXT @PageSize ROWS ONLY";
                }

                using (SqlCommand cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@SearchValue", searchValue ?? "");
                    cmd.Parameters.AddWithValue("@CategoryID", categoryID);
                    cmd.Parameters.AddWithValue("@SupplierID", supplierID);
                    cmd.Parameters.AddWithValue("@MinPrice", minPrice);
                    cmd.Parameters.AddWithValue("@MaxPrice", maxPrice);

                    if (pageSize > 0)
                    {
                        cmd.Parameters.AddWithValue("@Page", page);
                        cmd.Parameters.AddWithValue("@PageSize", pageSize);
                    }

                    using (SqlDataReader dbReader = cmd.ExecuteReader())
                    {
                        while (dbReader.Read())
                        {
                            data.Add(new Product()
                            {
                                ProductID = Convert.ToInt32(dbReader["ProductID"]),
                                ProductName = Convert.ToString(dbReader["ProductName"]),
                                ProductDescription = dbReader["ProductDescription"] != DBNull.Value ? Convert.ToString(dbReader["ProductDescription"]) : "",
                                SupplierID = dbReader["SupplierID"] != DBNull.Value ? Convert.ToInt32(dbReader["SupplierID"]) : 0,
                                CategoryID = dbReader["CategoryID"] != DBNull.Value ? Convert.ToInt32(dbReader["CategoryID"]) : 0,
                                Unit = dbReader["Unit"] != DBNull.Value ? Convert.ToString(dbReader["Unit"]) : "",
                                Price = dbReader["Price"] != DBNull.Value ? Convert.ToDecimal(dbReader["Price"]) : 0,
                                Photo = dbReader["Photo"] != DBNull.Value ? Convert.ToString(dbReader["Photo"]) : "",
                                IsSelling = dbReader["IsSelling"] != DBNull.Value ? Convert.ToBoolean(dbReader["IsSelling"]) : false
                            });
                        }
                    }
                }
            }
            return data;
        }

        // 2. Hàm lấy thông tin chi tiết của 1 mặt hàng theo ID
        public static Product? GetProduct(int productID)
        {
            Product? data = null;
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string sql = @"SELECT * FROM Products WHERE ProductID = @ProductID";
                using (SqlCommand cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@ProductID", productID);
                    using (SqlDataReader dbReader = cmd.ExecuteReader())
                    {
                        if (dbReader.Read())
                        {
                            data = new Product()
                            {
                                ProductID = Convert.ToInt32(dbReader["ProductID"]),
                                ProductName = Convert.ToString(dbReader["ProductName"]),
                                ProductDescription = dbReader["ProductDescription"] != DBNull.Value ? Convert.ToString(dbReader["ProductDescription"]) : "",
                                SupplierID = dbReader["SupplierID"] != DBNull.Value ? Convert.ToInt32(dbReader["SupplierID"]) : 0,
                                CategoryID = dbReader["CategoryID"] != DBNull.Value ? Convert.ToInt32(dbReader["CategoryID"]) : 0,
                                Unit = dbReader["Unit"] != DBNull.Value ? Convert.ToString(dbReader["Unit"]) : "",
                                Price = dbReader["Price"] != DBNull.Value ? Convert.ToDecimal(dbReader["Price"]) : 0,
                                Photo = dbReader["Photo"] != DBNull.Value ? Convert.ToString(dbReader["Photo"]) : "",
                                IsSelling = dbReader["IsSelling"] != DBNull.Value ? Convert.ToBoolean(dbReader["IsSelling"]) : false
                            };
                        }
                    }
                }
            }
            return data;
        }
    }
}