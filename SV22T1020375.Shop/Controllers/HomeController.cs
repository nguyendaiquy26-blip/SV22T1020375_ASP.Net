using Microsoft.AspNetCore.Mvc;
using SV22T1020375.BusinessLayers;
using System;

namespace SV22T1020375.Shop.Controllers
{
    public class HomeController : Controller
    {
        // Thêm các tham số để hứng dữ liệu từ Form tìm kiếm
        public IActionResult Index(int page = 1, string searchValue = "", int categoryID = 0, decimal minPrice = 0, decimal maxPrice = 0)
        {
            int pageSize = 8;
            int rowCount = 0;

            // Truyền các điều kiện tìm kiếm xuống Service
            var data = ProductDataService.ListOfProducts(out rowCount, page, pageSize, searchValue ?? "", categoryID, 0, minPrice, maxPrice);

            int totalPages = (int)Math.Ceiling((double)rowCount / pageSize);

            // Lưu lại thông tin phân trang
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            // LƯU LẠI trạng thái tìm kiếm để Form không bị mất dữ liệu khi load lại
            ViewBag.SearchValue = searchValue;
            ViewBag.CategoryID = categoryID;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;

            return View(data);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}