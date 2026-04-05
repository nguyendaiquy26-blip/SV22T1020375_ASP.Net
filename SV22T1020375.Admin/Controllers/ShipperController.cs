using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020375.BusinessLayers;
using SV22T1020375.Models.Common;
using SV22T1020375.Models.Partner;

namespace SV22T1020375.Admin.Controllers
{
    //[Authorize(Roles = "sales,admin")]
    //[Authorize(Roles = WebUserRoles.Sales + "," + WebUserRoles.Administrator)]
    /// <summary>
    /// Test phân quyền cho trang admin
    /// </summary>
    [Authorize(Roles = "sales")]
    public class ShipperController : Controller
    {
        public IActionResult Index()
        {
            var input = ApplicationContext.GetSessionData<PaginationSearchInput>("ShipperSearchInput");
            if (input == null)
                input = new PaginationSearchInput()
                {
                    Page = 1,
                    PageSize = ApplicationContext.PageSize,
                    SearchValue = ""
                };

            ViewBag.Title = "Đơn vị vận chuyển";
            return View(input);
        }

        public async Task<IActionResult> Search(PaginationSearchInput input)
        {
            if (input.Page < 1)
                input.Page = 1;

            if (input.PageSize <= 0)
                input.PageSize = ApplicationContext.PageSize;

            input.SearchValue ??= "";

            var result = await PartnerDataService.ListShippersAsync(input);
            ApplicationContext.SetSessionData("ShipperSearchInput", input);

            return PartialView(result);
        }

        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung đơn vị vận chuyển";
            var model = new Shipper()
            {
                ShipperID = 0
            };
            return View("Edit", model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Title = "Cập nhật đơn vị vận chuyển";
            var model = await PartnerDataService.GetShipperAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveData(Shipper data)
        {
            ViewBag.Title = data.ShipperID == 0
                ? "Bổ sung đơn vị vận chuyển"
                : "Cập nhật đơn vị vận chuyển";

            if (string.IsNullOrWhiteSpace(data.ShipperName))
                ModelState.AddModelError(nameof(data.ShipperName), "Vui lòng nhập tên đơn vị vận chuyển");

            if (string.IsNullOrWhiteSpace(data.Phone))
                ModelState.AddModelError(nameof(data.Phone), "Vui lòng nhập số điện thoại");

            if (!ModelState.IsValid)
                return View("Edit", data);

            if (data.ShipperID == 0)
                await PartnerDataService.AddShipperAsync(data);
            else
                await PartnerDataService.UpdateShipperAsync(data);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (Request.Method == "POST")
            {
                await PartnerDataService.DeleteShipperAsync(id);
                return RedirectToAction("Index");
            }

            ViewBag.Title = "Xóa đơn vị vận chuyển";

            var model = await PartnerDataService.GetShipperAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            ViewBag.AllowDelete = !(await PartnerDataService.IsUsedShipperAsync(id));

            return View(model);
        }
    }
}