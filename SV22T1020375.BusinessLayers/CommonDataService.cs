using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace SV22T1020375.BusinessLayers
{
    public static class CommonDataService
    {
        public static string ConnectionString { get; set; } = @"Server=.\SQLEXPRESS;Database=LiteCommerceDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;";

        /// <summary>
        /// Lấy danh sách Loại Hàng từ Database (Cho Dropdown Lọc)
        /// </summary>
        public static List<CategoryModel> ListOfCategories()
        {
            List<CategoryModel> data = new List<CategoryModel>();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string sql = "SELECT CategoryID, CategoryName FROM Categories";
                using (SqlCommand cmd = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader dbReader = cmd.ExecuteReader())
                    {
                        while (dbReader.Read())
                        {
                            data.Add(new CategoryModel()
                            {
                                CategoryID = Convert.ToInt32(dbReader["CategoryID"]),
                                CategoryName = Convert.ToString(dbReader["CategoryName"])
                            });
                        }
                    }
                }
            }
            return data;
        }

        /// <summary>
        /// Lấy danh sách các tỉnh/thành phố (Cho trang Đăng ký)
        /// </summary>
        public static List<ProvinceModel> ListOfProvinces()
        {
            return new List<ProvinceModel>
            {
                new ProvinceModel { ProvinceName = "Hà Nội" },
                new ProvinceModel { ProvinceName = "TP.Hồ Chí Minh" },
                new ProvinceModel { ProvinceName = "Đà Nẵng" },
                new ProvinceModel { ProvinceName = "Hải Phòng" },
                new ProvinceModel { ProvinceName = "Cần Thơ" },
                new ProvinceModel { ProvinceName = "Thừa Thiên Huế" },
                new ProvinceModel { ProvinceName = "Đồng Nai" },
                new ProvinceModel { ProvinceName = "Bình Dương" },
                new ProvinceModel { ProvinceName = "Cà Mau" }
            };
        }
    }

    public class CategoryModel
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; } = "";
    }

    public class ProvinceModel
    {
        public string ProvinceName { get; set; } = string.Empty;
    }
}