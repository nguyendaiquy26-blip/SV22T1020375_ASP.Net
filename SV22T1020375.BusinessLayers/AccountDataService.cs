using System.Threading.Tasks;
using SV22T1020375.DataLayers.Interfaces;
using SV22T1020375.DataLayers.SQLServer;
using SV22T1020375.Models.Security;
using SV22T1020375.Models.Partner; // Bổ sung namespace này để nhận diện lớp Customer

namespace SV22T1020375.BusinessLayers
{
    /// <summary>
    /// Các chức năng nghiệp vụ liên quan đến tài khoản (đăng nhập, đổi mật khẩu, đăng ký...)
    /// </summary>
    public static class AccountDataService
    {
        // Đã sửa lại đúng tên Interface của bạn
        private static IUserAccountRepository userAccountDB = null!;

        /// <summary>
        /// Khởi tạo dịch vụ (gọi trong Program.cs)
        /// </summary>
        /// <param name="connectionString">Chuỗi kết nối đến cơ sở dữ liệu</param>
        public static void Init(string connectionString)
        {
            // Đã sửa lại đúng tên class SQL Server của bạn
            userAccountDB = new UserAccountRepository(connectionString);
        }

        /// <summary>
        /// Xác thực tài khoản người dùng
        /// </summary>
        /// <param name="userName">Tên đăng nhập (Email)</param>
        /// <param name="password">Mật khẩu</param>
        /// <returns>Đối tượng UserAccount nếu thành công, null nếu thất bại</returns>
        public static async Task<UserAccount?> AuthorizeAsync(string userName, string password)
        {
            return await userAccountDB.AuthorizeAsync(userName, password);
        }

        /// <summary>
        /// Đổi mật khẩu tài khoản
        /// </summary>
        /// <param name="userName">Tên đăng nhập (Email)</param>
        /// <param name="newPassword">Mật khẩu mới</param>
        /// <returns>true nếu đổi thành công, false nếu thất bại</returns>
        public static async Task<bool> ChangePasswordAsync(string userName, string newPassword)
        {
            return await userAccountDB.ChangePasswordAsync(userName, newPassword);
        }

        /// <summary>
        /// Đăng ký tài khoản khách hàng mới
        /// </summary>
        /// <param name="data">Thông tin khách hàng</param>
        /// <param name="password">Mật khẩu</param>
        /// <returns>ID của khách hàng vừa đăng ký (trả về 0 nếu email đã tồn tại)</returns>
        public static async Task<int> RegisterCustomerAsync(Customer data, string password)
        {
            return await userAccountDB.RegisterCustomerAsync(data, password);
        }
    }
}