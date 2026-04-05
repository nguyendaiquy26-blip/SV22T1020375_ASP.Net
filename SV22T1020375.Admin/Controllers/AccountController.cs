using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020375.Admin;
using SV22T1020375.BusinessLayers;
using SV22T1020375.Models.Security;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SV22T1020375.Admin.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string username, string password)
        {
            ViewBag.Username = username;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("Error", "Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu");
                return View();
            }

            // Bước 1: Băm mật khẩu người dùng nhập vào ra MD5
            string hashedPassword = EncodeMD5(password);

            // Bước 2: Gọi đến AccountDataService với mật khẩu đã băm
            var userAccount = await AccountDataService.AuthorizeAsync(username, hashedPassword);

            if (userAccount == null)
            {
                ModelState.AddModelError("Error", "Tên đăng nhập hoặc mật khẩu không đúng");
                return View();
            }

            var userData = new WebUserData()
            {
                UserId = userAccount.UserId,
                UserName = userAccount.UserName,
                DisplayName = userAccount.DisplayName,
                Email = userAccount.Email,
                Photo = userAccount.Photo,
                Roles = string.IsNullOrWhiteSpace(userAccount.RoleNames)
                        ? new List<string>()
                        : userAccount.RoleNames.Split(',')
                                               .Select(r => r.Trim())
                                               .Where(r => !string.IsNullOrWhiteSpace(r))
                                               .ToList()
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                userData.CreatePrincipal()
            );

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(oldPassword) ||
                string.IsNullOrWhiteSpace(newPassword) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin";
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không đúng";
                return View();
            }

            string userName = User.FindFirst("Email")?.Value ?? "";
            if (string.IsNullOrWhiteSpace(userName))
            {
                ViewBag.Error = "Không xác định được tài khoản đăng nhập";
                return View();
            }

            // Băm mật khẩu cũ để kiểm tra xem có khớp với Database không
            string hashedOldPassword = EncodeMD5(oldPassword);
            var userAccount = await AccountDataService.AuthorizeAsync(userName, hashedOldPassword);

            if (userAccount == null)
            {
                ViewBag.Error = "Mật khẩu cũ không đúng";
                return View();
            }

            // Băm mật khẩu mới trước khi lưu xuống Database
            string hashedNewPassword = EncodeMD5(newPassword);
            bool result = await AccountDataService.ChangePasswordAsync(userName, hashedNewPassword);

            if (!result)
            {
                ViewBag.Error = "Đổi mật khẩu thất bại";
                return View();
            }

            ViewBag.Success = "Đổi mật khẩu thành công";
            return View();
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // Hàm hỗ trợ mã hóa MD5
        private string EncodeMD5(string pass)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(pass);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                var sb = new System.Text.StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2")); // x2 để in thường các ký tự hex
                }
                return sb.ToString();
            }
        }
    }
}