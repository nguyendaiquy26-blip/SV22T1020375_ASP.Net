using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace SV22T1020375.BusinessLayers
{
    public static class CustomerDataService
    {
        public static string ConnectionString { get; set; } = @"Server=.\SQLEXPRESS;Database=LiteCommerceDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true;";

        // Cập nhật: Trả về List<CustomerModel> thay vì List<dynamic>
        public static List<CustomerModel> ListCustomers()
        {
            List<CustomerModel> data = new List<CustomerModel>();
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                string sql = @"SELECT CustomerID, CustomerName FROM Customers ORDER BY CustomerName";
                using (SqlCommand cmd = new SqlCommand(sql, connection))
                {
                    using (SqlDataReader dbReader = cmd.ExecuteReader())
                    {
                        while (dbReader.Read())
                        {
                            // Cập nhật: Khởi tạo đối tượng từ class CustomerModel
                            data.Add(new CustomerModel()
                            {
                                CustomerID = Convert.ToInt32(dbReader["CustomerID"]),
                                CustomerName = Convert.ToString(dbReader["CustomerName"])
                            });
                        }
                    }
                }
            }
            return data;
        }
    }

    // Bổ sung: Định nghĩa class CustomerModel để truyền dữ liệu an toàn sang View
    public class CustomerModel
    {
        public int CustomerID { get; set; }
        public string CustomerName { get; set; } = "";
    }
}