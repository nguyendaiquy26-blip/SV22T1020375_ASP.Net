using SV22T1020375.Models.Partner;

namespace SV22T1020375.DataLayers.Interfaces
{
    /// <summary>
    /// Định nghĩa các phép xử lý dữ liệu trên Customer
    /// </summary>
    public interface ICustomerRepository : IGenericRepository<Customer>
    {
        /// <summary>
        /// Kiểm tra xem một địa chỉ email có hợp lệ hay không? (Không bị trùng)
        /// </summary>
        Task<bool> ValidateEmailAsync(string email, int id = 0);

        /// <summary>
        /// Đăng ký khách hàng mới có mật khẩu
        /// </summary>
        Task<int> RegisterAsync(Customer data, string password);

        /// <summary>
        /// Đổi mật khẩu khách hàng
        /// </summary>
        Task<bool> ChangePasswordAsync(int customerID, string password);
    }
}