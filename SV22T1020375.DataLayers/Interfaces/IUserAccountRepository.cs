using SV22T1020375.Models.Partner;
using SV22T1020375.Models.Security;
using System.Threading.Tasks;

namespace SV22T1020375.DataLayers.Interfaces
{
    /// <summary>
    /// Định nghĩa các phép xử lý dữ liệu liên quan đến tài khoản
    /// </summary>
    public interface IUserAccountRepository
    {
        /// <summary>
        /// Kiểm tra xem tên đăng nhập và mật khẩu có hợp lệ không
        /// </summary>
        Task<UserAccount?> AuthorizeAsync(string userName, string password);

        /// <summary>
        /// Đăng ký tài khoản khách hàng mới
        /// </summary>
        Task<int> RegisterCustomerAsync(Customer data, string password);

        /// <summary>
        /// Đổi mật khẩu của tài khoản
        /// </summary>
        Task<bool> ChangePasswordAsync(string userName, string password);
    }
}