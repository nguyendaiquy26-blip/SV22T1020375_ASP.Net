using SV22T1020375.DataLayers.Interfaces;
using SV22T1020375.DataLayers.SQLServer;
using SV22T1020375.Models.Partner;
using SV22T1020375.Models.Security;
using System;
using System.Threading.Tasks;

namespace SV22T1020375.BusinessLayers
{
    public static class UserAccountService
    {
        private static IUserAccountRepository? _userAccountDB;

        public static void Initialize(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("ConnectionString không được rỗng khi khởi tạo UserAccountService.");

            _userAccountDB = new UserAccountRepository(connectionString);
        }

        public static async Task<UserAccount?> AuthorizeAsync(string userName, string password)
        {
            if (_userAccountDB == null)
                throw new InvalidOperationException("UserAccountService chưa được khởi tạo. Hãy gọi UserAccountService.Initialize() trong Program.cs.");

            return await _userAccountDB.AuthorizeAsync(userName, password);
        }

        public static async Task<int> RegisterCustomerAsync(Customer data, string password)
        {
            if (_userAccountDB == null)
                throw new InvalidOperationException("UserAccountService chưa được khởi tạo.");

            if (string.IsNullOrWhiteSpace(data.Email))
                throw new ArgumentException("Email không được để trống.");

            int customerId = await _userAccountDB.RegisterCustomerAsync(data, password);

            return customerId;
        }

        public static async Task<bool> ChangePasswordAsync(string userName, string oldPassword, string newPassword)
        {
            if (_userAccountDB == null)
                throw new InvalidOperationException("UserAccountService chưa được khởi tạo.");

            var user = await AuthorizeAsync(userName, oldPassword);
            if (user == null)
                return false;

            return await _userAccountDB.ChangePasswordAsync(userName, newPassword);
        }
    }
}