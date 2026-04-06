using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020375.BusinessLayers;
using SV22T1020375.Models.Partner;
using System.Security.Claims;
// Thêm 2 thư viện này để sử dụng mã hóa MD5
using System.Security.Cryptography;
using System.Text;

namespace SV22T1020375.Shop.Controllers
{
    public class AccountController : Controller
    {
        /// <summary>
        /// Hàm hỗ trợ mã hóa MD5
        /// </summary>
        private string GetMd5Hash(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ thông tin");
                return View();
            }

            // [ĐÃ SỬA]: Mã hóa mật khẩu người dùng nhập vào
            string hashedPassword = GetMd5Hash(password);

            // [ĐÃ SỬA]: Truyền hashedPassword vào AuthorizeAsync thay vì password gốc
            var userAccount = await UserAccountService.AuthorizeAsync(username, hashedPassword);

            if (userAccount == null)
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng");
                return View();
            }

            if (string.IsNullOrEmpty(userAccount.RoleNames) || !userAccount.RoleNames.Contains("Customer"))
            {
                ModelState.AddModelError("", "Tài khoản không có quyền truy cập cửa hàng.");
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userAccount.DisplayName ?? ""),
                new Claim(ClaimTypes.Email, userAccount.Email ?? ""),
                new Claim(ClaimTypes.NameIdentifier, userAccount.UserId ?? ""),
                new Claim(ClaimTypes.Role, userAccount.RoleNames ?? ""),
                new Claim("Photo", userAccount.Photo ?? "")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(Customer data, string password, string confirmPassword)
        {
            // 1. Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(data.CustomerName))
                ModelState.AddModelError(nameof(data.CustomerName), "Tên không được để trống");
            if (string.IsNullOrWhiteSpace(data.Email))
                ModelState.AddModelError(nameof(data.Email), "Email không được để trống");
            if (string.IsNullOrWhiteSpace(password))
                ModelState.AddModelError("password", "Mật khẩu không được để trống");
            if (password != confirmPassword)
                ModelState.AddModelError("confirmPassword", "Mật khẩu xác nhận không khớp");

            if (!ModelState.IsValid)
            {
                return View(data);
            }

            // 2. Khử null
            data.CustomerName = data.CustomerName ?? "";
            data.ContactName = data.ContactName ?? "";
            data.Province = data.Province ?? "";
            data.Address = data.Address ?? "";
            data.Phone = data.Phone ?? "";
            data.Email = data.Email ?? "";

            try
            {
                // 3. MÃ HÓA MẬT KHẨU TRƯỚC KHI LƯU
                string hashedPassword = GetMd5Hash(password);

                // Gọi Business Layer để lưu (truyền mật khẩu đã mã hóa)
                int id = await UserAccountService.RegisterCustomerAsync(data, hashedPassword);
                if (id == 0)
                {
                    ModelState.AddModelError(nameof(data.Email), "Email này đã được sử dụng");
                    return View(data);
                }

                // 4. ĐĂNG KÝ XONG TỰ ĐỘNG ĐĂNG NHẬP
                // [ĐÃ SỬA]: Truyền hashedPassword thay vì password gốc
                var userAccount = await UserAccountService.AuthorizeAsync(data.Email, hashedPassword);

                if (userAccount != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, userAccount.DisplayName ?? ""),
                        new Claim(ClaimTypes.Email, userAccount.Email ?? ""),
                        new Claim(ClaimTypes.NameIdentifier, userAccount.UserId ?? ""),
                        new Claim(ClaimTypes.Role, userAccount.RoleNames ?? ""),
                        new Claim("Photo", userAccount.Photo ?? "")
                    };

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                    return RedirectToAction("Index", "Home");
                }

                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi hệ thống: " + ex.Message);
                return View(data);
            }
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            int customerId = 0;
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out customerId);

            var customer = await PartnerDataService.GetCustomerAsync(customerId);
            if (customer == null)
                return RedirectToAction("Logout");

            return View(customer);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Profile(Customer data)
        {
            if (string.IsNullOrWhiteSpace(data.CustomerName))
                ModelState.AddModelError(nameof(data.CustomerName), "Tên không được để trống");

            if (!ModelState.IsValid)
                return View(data);

            int customerId = 0;
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out customerId);
            data.CustomerID = customerId;

            data.ContactName = data.ContactName ?? "";
            data.Province = data.Province ?? "";
            data.Address = data.Address ?? "";
            data.Phone = data.Phone ?? "";

            bool isValidEmail = await PartnerDataService.ValidatelCustomerEmailAsync(data.Email, data.CustomerID);
            if (!isValidEmail)
            {
                ModelState.AddModelError(nameof(data.Email), "Email này đã được sử dụng");
                return View(data);
            }

            bool result = await PartnerDataService.UpdateCustomerAsync(data);
            if (!result)
            {
                ModelState.AddModelError("", "Cập nhật thất bại.");
                return View(data);
            }

            ViewBag.SuccessMessage = "Cập nhật thông tin thành công!";
            return View(data);
        }

        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(oldPassword))
                ModelState.AddModelError("oldPassword", "Vui lòng nhập mật khẩu hiện tại");
            if (string.IsNullOrWhiteSpace(newPassword))
                ModelState.AddModelError("newPassword", "Vui lòng nhập mật khẩu mới");
            if (newPassword != confirmPassword)
                ModelState.AddModelError("confirmPassword", "Mật khẩu xác nhận không khớp");

            if (!ModelState.IsValid)
                return View();

            string userName = User.FindFirstValue(ClaimTypes.Email) ?? "";

            // [ĐÃ SỬA]: Mở khóa và sử dụng MD5 để mã hóa mật khẩu cũ và mới
            string hashedOld = GetMd5Hash(oldPassword);
            string hashedNew = GetMd5Hash(newPassword);

            // [ĐÃ SỬA]: Truyền mật khẩu đã mã hóa vào hàm đổi mật khẩu
            bool result = await UserAccountService.ChangePasswordAsync(userName, hashedOld, hashedNew);

            if (!result)
            {
                ModelState.AddModelError("oldPassword", "Mật khẩu hiện tại không đúng");
                return View();
            }

            ViewBag.SuccessMessage = "Đổi mật khẩu thành công!";
            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}