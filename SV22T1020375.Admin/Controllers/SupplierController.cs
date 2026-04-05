using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020375.BusinessLayers;
using SV22T1020375.Models.Common;
using SV22T1020375.Models.Partner;

namespace SV22T1020375.Admin.Controllers
{
    [Authorize(Roles = "sales,admin")]
    public class SupplierController : Controller
    {
        //private const int PAGE_SIZE = 10;
        [Authorize(Roles = WebUserRoles.Sales + "," + WebUserRoles.Administrator)]
        public IActionResult Index()
        {
            var input = ApplicationContext.GetSessionData<PaginationSearchInput>("SupplierSearchInput");
            if (input == null)
                input = new PaginationSearchInput()
                {
                    Page = 1,
                    PageSize = ApplicationContext.PageSize,
                    SearchValue = ""
                };

            return View(input);
        }

        public async Task<IActionResult> Search(PaginationSearchInput input)
        {
            if (input.Page < 1)
                input.Page = 1;

            if (input.PageSize <= 0)
                input.PageSize = ApplicationContext.PageSize;

            input.SearchValue ??= "";

            var result = await PartnerDataService.ListSuppliersAsync(input);
            ApplicationContext.SetSessionData("SupplierSearchInput", input);

            return PartialView(result);
        }


        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung Nhà cung cấp";

            var model = new Supplier()
            {
                SupplierID = 0
            };

            return View("Edit", model);
        }


        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Title = "Cập nhật thông tin Nhà cung cấp";
            var model = await PartnerDataService.GetSupplierAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> SaveData(Supplier data)
        {
            ViewBag.Title = data.SupplierID == 0
                ? "Bổ sung Nhà cung cấp"
                : "Cập nhật thông tin Nhà cung cấp";

            if (string.IsNullOrWhiteSpace(data.SupplierName))
            {
                ModelState.AddModelError(nameof(data.SupplierName), "Tên nhà cung cấp không được để trống");
            }

            if (string.IsNullOrWhiteSpace(data.ContactName))
            {
                ModelState.AddModelError(nameof(data.ContactName), "Tên giao dịch không được để trống");
            }

            if (string.IsNullOrWhiteSpace(data.Province))
            {
                ModelState.AddModelError(nameof(data.Province), "Vui lòng chọn tỉnh/thành");
            }
            if (string.IsNullOrWhiteSpace(data.Email))
                ModelState.AddModelError(nameof(data.Email), "Vui lòng nhập email");

            if (string.IsNullOrEmpty(data.Phone)) data.Phone = "";
            if (string.IsNullOrEmpty(data.Address)) data.Address = "";

            if (!ModelState.IsValid)
                return View("Edit", data);

            if (data.SupplierID == 0)
                await PartnerDataService.AddSupplierAsync(data);
            else
                await PartnerDataService.UpdateSupplierAsync(data);

            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Delete(int id)
        {
            if (Request.Method == "POST")
            {
                await PartnerDataService.DeleteSupplierAsync(id);
                return RedirectToAction("Index");
            }

            var model = await PartnerDataService.GetSupplierAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            ViewBag.AllowDelete = !(await PartnerDataService.IsUsedSupplierAsync(id));

            return View(model);
        }
    }
}