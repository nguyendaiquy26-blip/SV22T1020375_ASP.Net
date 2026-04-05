using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020375.BusinessLayers;
using SV22T1020375.Models.Common;
using SV22T1020375.Models.HR;

namespace SV22T1020375.Admin.Controllers
{
    [Authorize(Roles = WebUserRoles.Sales + "," + WebUserRoles.Administrator)]
    public class EmployeeController : Controller
    {
        public IActionResult Index()
        {
            var input = ApplicationContext.GetSessionData<PaginationSearchInput>("EmployeeSearchInput");
            if (input == null)
                input = new PaginationSearchInput()
                {
                    Page = 1,
                    PageSize = ApplicationContext.PageSize,
                    SearchValue = ""
                };

            ViewBag.Title = "Quản lý Nhân viên";
            return View(input);
        }

        public async Task<IActionResult> Search(PaginationSearchInput input)
        {
            if (input.Page < 1)
                input.Page = 1;

            if (input.PageSize <= 0)
                input.PageSize = ApplicationContext.PageSize;

            input.SearchValue ??= "";

            var result = await HRDataService.ListEmployeesAsync(input);
            ApplicationContext.SetSessionData("EmployeeSearchInput", input);

            return PartialView(result);
        }

        public IActionResult Create()
        {
            ViewBag.Title = "Thêm nhân viên";
            var model = new Employee()
            {
                EmployeeID = 0,
                IsWorking = true,
                Photo = ""
            };
            return View("Edit", model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Title = "Cập nhật thông tin nhân viên";

            var data = await HRDataService.GetEmployeeAsync(id);
            if (data == null)
                return RedirectToAction("Index");

            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> SaveData(Employee data, IFormFile? uploadPhoto)
        {
            ViewBag.Title = data.EmployeeID == 0 ? "Thêm nhân viên" : "Cập nhật thông tin nhân viên";

            if (string.IsNullOrWhiteSpace(data.FullName))
                ModelState.AddModelError(nameof(data.FullName), "Vui lòng nhập họ và tên");

            if (data.BirthDate == null)
                ModelState.AddModelError(nameof(data.BirthDate), "Vui lòng nhập ngày sinh");

            if (string.IsNullOrWhiteSpace(data.Address))
                ModelState.AddModelError(nameof(data.Address), "Vui lòng nhập địa chỉ");

            if (string.IsNullOrWhiteSpace(data.Phone))
                ModelState.AddModelError(nameof(data.Phone), "Vui lòng nhập số điện thoại");

            if (string.IsNullOrWhiteSpace(data.Email))
                ModelState.AddModelError(nameof(data.Email), "Vui lòng nhập email");

            if (!ModelState.IsValid)
                return View("Edit", data);

            if (uploadPhoto != null && uploadPhoto.Length > 0)
            {
                string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "employees");
                Directory.CreateDirectory(folder);

                string fileName = $"{DateTime.Now.Ticks}{Path.GetExtension(uploadPhoto.FileName)}";
                string filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await uploadPhoto.CopyToAsync(stream);
                }

                data.Photo = fileName;
            }

            if (data.EmployeeID == 0)
                await HRDataService.AddEmployeeAsync(data);
            else
                await HRDataService.UpdateEmployeeAsync(data);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (Request.Method == "POST")
            {
                await HRDataService.DeleteEmployeeAsync(id);
                return RedirectToAction("Index");
            }

            ViewBag.Title = "Xóa nhân viên";

            var data = await HRDataService.GetEmployeeAsync(id);
            if (data == null)
                return RedirectToAction("Index");

            ViewBag.AllowDelete = !(await HRDataService.IsUsedEmployeeAsync(id));
            return View(data);
        }

        public async Task<IActionResult> ChangePassword(int id)
        {
            ViewBag.Title = "Đổi mật khẩu nhân viên";

            var data = await HRDataService.GetEmployeeAsync(id);
            if (data == null)
                return RedirectToAction("Index");

            return View(data);
        }

        public async Task<IActionResult> ChangeRole(int id)
        {
            ViewBag.Title = "Phân quyền nhân viên";

            var data = await HRDataService.GetEmployeeAsync(id);
            if (data == null)
                return RedirectToAction("Index");

            return View(data);
        }
    }
}