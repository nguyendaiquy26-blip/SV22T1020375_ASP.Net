using Microsoft.AspNetCore.Mvc;
using SV22T1020375.BusinessLayers;
using SV22T1020375.Models.Common;
using SV22T1020375.Models.Partner; // Gọi namespace chứa ChangeCustomerPasswordViewModel

namespace SV22T1020375.Admin.Controllers
{
    public class CustomerController : Controller
    {
        // 1. GỌI TRANG CHỦ & THANH TÌM KIẾM
        public IActionResult Index()
        {
            var input = new PaginationSearchInput()
            {
                Page = 1,
                PageSize = 20,
                SearchValue = ""
            };
            return View(input);
        }

        // 2. TÌM KIẾM DỮ LIỆU
        public async Task<IActionResult> Search(PaginationSearchInput condition)
        {
            // Sử dụng đúng tên hàm ListCustomersAsync
            var data = await PartnerDataService.ListCustomersAsync(condition);
            return View(data);
        }

        // 3. THÊM MỚI KHÁCH HÀNG (Dùng chung Edit.cshtml)
        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung khách hàng mới";
            var model = new Customer()
            {
                CustomerID = 0,
                IsLocked = false
            };
            return View("Edit", model);
        }

        // 4. CẬP NHẬT KHÁCH HÀNG (Mở Edit.cshtml)
        public async Task<IActionResult> Edit(int id = 0)
        {
            ViewBag.Title = "Cập nhật thông tin khách hàng";
            // Sử dụng đúng tên hàm GetCustomerAsync
            var model = await PartnerDataService.GetCustomerAsync(id);

            if (model == null)
                return RedirectToAction("Index");

            return View(model);
        }

        // 5. LƯU DỮ LIỆU TỪ FILE EDIT.CSHTML
        [HttpPost]
        public async Task<IActionResult> SaveData(Customer data)
        {
            // Tránh lỗi null reference
            data.CustomerName = string.IsNullOrWhiteSpace(data.CustomerName) ? "" : data.CustomerName;
            data.ContactName = string.IsNullOrWhiteSpace(data.ContactName) ? "" : data.ContactName;
            data.Phone = string.IsNullOrWhiteSpace(data.Phone) ? "" : data.Phone;
            data.Email = string.IsNullOrWhiteSpace(data.Email) ? "" : data.Email;
            data.Address = string.IsNullOrWhiteSpace(data.Address) ? "" : data.Address;
            data.Province = string.IsNullOrWhiteSpace(data.Province) ? "" : data.Province;

            if (string.IsNullOrWhiteSpace(data.CustomerName))
                ModelState.AddModelError(nameof(data.CustomerName), "Tên khách hàng không được để trống");

            if (!ModelState.IsValid)
            {
                ViewBag.Title = data.CustomerID == 0 ? "Bổ sung khách hàng" : "Cập nhật thông tin khách hàng";
                return View("Edit", data);
            }

            // Sử dụng đúng các hàm AddCustomerAsync và UpdateCustomerAsync
            if (data.CustomerID == 0)
            {
                await PartnerDataService.AddCustomerAsync(data);
            }
            else
            {
                await PartnerDataService.UpdateCustomerAsync(data);
            }

            return RedirectToAction("Index");
        }

        // 6. XÓA KHÁCH HÀNG
        public async Task<IActionResult> Delete(int id = 0)
        {
            if (Request.Method == "POST")
            {
                await PartnerDataService.DeleteCustomerAsync(id);
                return RedirectToAction("Index");
            }

            var model = await PartnerDataService.GetCustomerAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            // Sử dụng đúng hàm IsUsedCustomerAsync để kiểm tra trước khi xóa
            ViewBag.AllowDelete = !await PartnerDataService.IsUsedCustomerAsync(id);

            return View(model);
        }

        // 7. ĐỔI MẬT KHẨU
        [HttpGet]
        public async Task<IActionResult> ChangePassword(int id)
        {
            var customer = await PartnerDataService.GetCustomerAsync(id);
            if (customer == null)
                return RedirectToAction("Index");

            // Khởi tạo Model gửi sang ChangePassword.cshtml
            var model = new ChangeCustomerPasswordViewModel()
            {
                CustomerID = customer.CustomerID,
                CustomerName = customer.CustomerName ?? "",
                Email = customer.Email ?? "",
                IsLocked = customer.IsLocked ?? false
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangeCustomerPasswordViewModel model)
        {
            // Kiểm tra mật khẩu xác nhận có khớp không
            if (model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError(nameof(model.ConfirmPassword), "Mật khẩu xác nhận không khớp.");
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Gọi đúng hàm ChangeCustomerPasswordAsync
            await PartnerDataService.ChangeCustomerPasswordAsync(model.CustomerID, model.NewPassword);

            return RedirectToAction("Index");
        }
    }
}

// ======================================================================
// THỦ THUẬT: Đặt class này ở ngay cuối file CustomerController.cs 
// để đáp ứng yêu cầu của file ChangePassword.cshtml mà KHÔNG cần tạo file mới
// ======================================================================
namespace SV22T1020375.Models.Partner
{
    public class ChangeCustomerPasswordViewModel
    {
        public int CustomerID { get; set; }
        public string CustomerName { get; set; } = "";
        public string Email { get; set; } = "";
        public bool IsLocked { get; set; }

        public string NewPassword { get; set; } = "";
        public string ConfirmPassword { get; set; } = "";
    }
}