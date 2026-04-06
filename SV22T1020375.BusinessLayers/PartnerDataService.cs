using SV22T1020375.DataLayers.Interfaces;
using SV22T1020375.DataLayers.SQLServer;
using SV22T1020375.Models.Common;
using SV22T1020375.Models.Partner;

namespace SV22T1020375.BusinessLayers
{
    /// <summary>
    /// Cung cấp các chức năng xử lý dữ liệu liên quan đến các đối tác của hệ thống
    /// bao gồm: nhà cung cấp (Supplier), khách hàng (Customer) và người giao hàng (Shipper)
    /// </summary>
    public static class PartnerDataService
    {
        private static readonly IGenericRepository<Supplier> supplierDB;
        private static readonly ICustomerRepository customerDB;
        private static readonly IGenericRepository<Shipper> shipperDB;

        /// <summary>
        /// Ctor
        /// </summary>
        static PartnerDataService()
        {
            supplierDB = new SupplierRepository(Configuration.ConnectionString);
            customerDB = new CustomerRepository(Configuration.ConnectionString);
            shipperDB = new ShipperRepository(Configuration.ConnectionString);
        }

        #region Supplier

        public static async Task<PagedResult<Supplier>> ListSuppliersAsync(PaginationSearchInput input)
        {
            return await supplierDB.ListAsync(input);
        }

        public static async Task<Supplier?> GetSupplierAsync(int supplierID)
        {
            return await supplierDB.GetAsync(supplierID);
        }

        public static async Task<int> AddSupplierAsync(Supplier data)
        {
            return await supplierDB.AddAsync(data);
        }

        public static async Task<bool> UpdateSupplierAsync(Supplier data)
        {
            return await supplierDB.UpdateAsync(data);
        }

        public static async Task<bool> DeleteSupplierAsync(int supplierID)
        {
            if (await supplierDB.IsUsedAsync(supplierID))
                return false;

            return await supplierDB.DeleteAsync(supplierID);
        }

        public static async Task<bool> IsUsedSupplierAsync(int supplierID)
        {
            return await supplierDB.IsUsedAsync(supplierID);
        }

        #endregion

        #region Customer

        public static async Task<PagedResult<Customer>> ListCustomersAsync(PaginationSearchInput input)
        {
            return await customerDB.ListAsync(input);
        }

        public static async Task<Customer?> GetCustomerAsync(int customerID)
        {
            return await customerDB.GetAsync(customerID);
        }

        public static async Task<int> AddCustomerAsync(Customer data)
        {
            return await customerDB.AddAsync(data);
        }

        public static async Task<bool> UpdateCustomerAsync(Customer data)
        {
            return await customerDB.UpdateAsync(data);
        }

        public static async Task<bool> DeleteCustomerAsync(int customerID)
        {
            if (await customerDB.IsUsedAsync(customerID))
                return false;

            return await customerDB.DeleteAsync(customerID);
        }

        public static async Task<bool> IsUsedCustomerAsync(int customerID)
        {
            // Ép kiểu tường minh để tránh lỗi bool?
            bool result = await customerDB.IsUsedAsync(customerID);
            return result;
        }

        public static async Task<bool> ValidatelCustomerEmailAsync(string email, int customerID = 0)
        {
            return await customerDB.ValidateEmailAsync(email, customerID);
        }

        /// <summary>
        /// Đổi mật khẩu khách hàng
        /// </summary>
        public static async Task<bool> ChangeCustomerPasswordAsync(int customerID, string newPassword)
        {
            // Sử dụng await để tránh lỗi "lacks 'await' operators"
            return await customerDB.ChangePasswordAsync(customerID, newPassword);
        }

        #endregion

        #region Shipper

        public static async Task<PagedResult<Shipper>> ListShippersAsync(PaginationSearchInput input)
        {
            return await shipperDB.ListAsync(input);
        }

        public static async Task<Shipper?> GetShipperAsync(int shipperID)
        {
            return await shipperDB.GetAsync(shipperID);
        }

        public static async Task<int> AddShipperAsync(Shipper data)
        {
            return await shipperDB.AddAsync(data);
        }

        public static async Task<bool> UpdateShipperAsync(Shipper data)
        {
            return await shipperDB.UpdateAsync(data);
        }

        public static async Task<bool> DeleteShipperAsync(int shipperID)
        {
            if (await shipperDB.IsUsedAsync(shipperID))
                return false;

            return await shipperDB.DeleteAsync(shipperID);
        }

        public static async Task<bool> IsUsedShipperAsync(int shipperID)
        {
            return await shipperDB.IsUsedAsync(shipperID);
        }

        #endregion
    }
}