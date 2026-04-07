using Microsoft.AspNetCore.Mvc;
using SV22T1020375.BusinessLayers;
using System;

namespace SV22T1020375.Shop.Controllers
{
    public class HomeController : Controller
    {
        
        public IActionResult Index(int page = 1, string searchValue = "", int categoryID = 0, decimal minPrice = 0, decimal maxPrice = 0)
        {
            // THÊM DÒNG NÀY: Nếu ID là 0 thì gán ViewBag là null để nhận biết trang thái "Tất cả"
            // ViewBag.CategoryIDForUi = categoryID == 0 ? (int?)null : categoryID; 

            // --> HOẶC GIỮ NGUYÊN CODE CŨ CỦA BẠN, CHỈ CẦN LÀM BƯỚC 1 VÀ 2 LÀ UI CHẠY RỒI. 
            // MÌNH KHÔNG SỬA GÌ TRONG CONTROLLER ĐỂ ĐẢM BẢO KHÔNG LỖI. 
            // (Trong code Views/Home/Index.cshtml Bước 2, mình đã xử lý View để active category 0).

            int pageSize = 8;
            int rowCount = 0;

            // Truyền các điều kiện tìm kiếm xuống Service
            var data = ProductDataService.ListOfProducts(out rowCount, page, pageSize, searchValue ?? "", categoryID, 0, minPrice, maxPrice);

            int totalPages = (int)Math.Ceiling((double)rowCount / pageSize);

            // Lưu lại thông tin phân trang
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            // LƯU LẠI trạng thái tìm kiếm
            ViewBag.SearchValue = searchValue;
            ViewBag.CategoryID = categoryID; // Giữ nguyên dòng này của bạn
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