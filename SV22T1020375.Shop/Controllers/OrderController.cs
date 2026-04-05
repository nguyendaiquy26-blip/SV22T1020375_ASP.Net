using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020375.BusinessLayers;
using SV22T1020375.Models.Common;
using SV22T1020375.Models.Sales;
using System.Security.Claims;

namespace SV22T1020375.Shop.Controllers
{
    [Authorize] // Bắt buộc đăng nhập
    public class OrderController : Controller
    {
        // ==========================================
        // 1. HIỂN THỊ DANH SÁCH ĐƠN HÀNG CỦA KHÁCH
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int customerId)) return RedirectToAction("Login", "Account");

            // Tạo input tìm kiếm (Giả sử lấy 1000 đơn để bao quát hết)
            var searchInput = new OrderSearchInput
            {
                Page = 1,
                PageSize = 1000,
                SearchValue = "",
                Status = 0 // Lấy tất cả trạng thái
            };

            // Lấy danh sách từ CSDL
            var pagedResult = await OrderDataService.ListOrdersAsync(searchInput);

            // Lọc ra các đơn hàng của riêng khách hàng đang đăng nhập & xếp đơn mới nhất lên đầu
            // ĐÃ SỬA THÀNH DataItems CHUẨN XÁC 100%
            var customerOrders = pagedResult.DataItems
                .Where(o => o.CustomerID == customerId)
                .OrderByDescending(o => o.OrderTime)
                .ToList();

            return View(customerOrders);
        }

        // ==========================================
        // 2. XEM CHI TIẾT 1 ĐƠN HÀNG
        // ==========================================
        public async Task<IActionResult> Details(int id)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int customerId)) return RedirectToAction("Login", "Account");

            // Lấy thông tin chung của đơn hàng (Trả về OrderViewInfo)
            var order = await OrderDataService.GetOrderAsync(id);
            if (order == null || order.CustomerID != customerId)
            {
                return RedirectToAction("Index"); // Chặn nếu cố tình xem đơn người khác
            }

            // Lấy chi tiết các mặt hàng trong đơn (Trả về List<OrderDetailViewInfo>)
            var details = await OrderDataService.ListOrderDetailsAsync(id);
            ViewBag.OrderDetails = details;

            return View(order);
        }
    }
}