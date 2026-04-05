using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020375.BusinessLayers;
using SV22T1020375.Models.Catalog;
using SV22T1020375.Models.Common;

namespace SV22T1020375.Admin.Controllers
{
    public class CategoryController : Controller
    {
        [Authorize(Roles = WebUserRoles.Sales + "," + WebUserRoles.Administrator)]
        public IActionResult Index()
        {
            var input = ApplicationContext.GetSessionData<PaginationSearchInput>("CategorySearchInput");
            if (input == null)
                input = new PaginationSearchInput()
                {
                    Page = 1,
                    PageSize = ApplicationContext.PageSize,
                    SearchValue = ""
                };

            ViewBag.Title = "Quản lý Loại hàng";
            return View(input);
        }

        public async Task<IActionResult> Search(PaginationSearchInput input)
        {
            if (input.Page < 1)
                input.Page = 1;

            if (input.PageSize <= 0)
                input.PageSize = ApplicationContext.PageSize;

            input.SearchValue ??= "";

            var result = await CatalogDataService.ListCategoriesAsync(input);
            ApplicationContext.SetSessionData("CategorySearchInput", input);

            return PartialView(result);
        }

        public IActionResult Create()
        {
            ViewBag.Title = "Thêm loại hàng";
            var model = new Category()
            {
                CategoryID = 0
            };
            return View("Edit", model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Title = "Cập nhật loại hàng";

            var data = await CatalogDataService.GetCategoryAsync(id);
            if (data == null)
                return RedirectToAction("Index");

            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> SaveData(Category data)
        {
            ViewBag.Title = data.CategoryID == 0 ? "Thêm loại hàng" : "Cập nhật loại hàng";

            if (string.IsNullOrWhiteSpace(data.CategoryName))
                ModelState.AddModelError(nameof(data.CategoryName), "Vui lòng nhập tên loại hàng");

            if (string.IsNullOrEmpty(data.Description))
                data.Description = "";

            if (!ModelState.IsValid)
                return View("Edit", data);

            if (data.CategoryID == 0)
                await CatalogDataService.AddCategoryAsync(data);
            else
                await CatalogDataService.UpdateCategoryAsync(data);

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (Request.Method == "POST")
            {
                await CatalogDataService.DeleteCategoryAsync(id);
                return RedirectToAction("Index");
            }

            ViewBag.Title = "Xóa loại hàng";

            var data = await CatalogDataService.GetCategoryAsync(id);
            if (data == null)
                return RedirectToAction("Index");

            ViewBag.AllowDelete = !(await CatalogDataService.IsUsedCategoryAsync(id));
            return View(data);
        }
    }
}