using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using SV22T1020375.BusinessLayers;
using SV22T1020375.Models;
using Microsoft.AspNetCore.Http; // Cần thiết để dùng Session

namespace SV22T1020375.Shop.Controllers
{
    public class ProductController : Controller
    {
        private const string SHOPPING_CART = "SHOPPING_CART";

        // ==========================================
        // CÁC HÀM HỖ TRỢ (Dùng nội bộ để code gọn hơn)
        // ==========================================
        private List<CartItem> GetCart()
        {
            var sessionData = HttpContext.Session.GetString(SHOPPING_CART);
            if (sessionData != null)
            {
                return JsonSerializer.Deserialize<List<CartItem>>(sessionData);
            }
            return new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            var sessionData = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString(SHOPPING_CART, sessionData);
        }

        // ==========================================
        // 1. HIỂN THỊ CHI TIẾT SẢN PHẨM 
        // ==========================================
        public async Task<IActionResult> Details(int id = 0)
        {
            var product = await CatalogDataService.GetProductAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // ==========================================
        // 2. HIỂN THỊ TRANG GIỎ HÀNG
        // ==========================================
        public IActionResult ShoppingCart()
        {
            return View(GetCart());
        }

        // ==========================================
        // 3. THÊM VÀO GIỎ HÀNG 
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductID == productId);

            if (item != null)
            {
                // Nếu sản phẩm đã có trong giỏ, chỉ cần cộng thêm số lượng
                item.Quantity += quantity;
            }
            else
            {
                var product = await CatalogDataService.GetProductAsync(productId);
                if (product != null)
                {
                    cart.Add(new CartItem
                    {
                        ProductID = product.ProductID,
                        ProductName = product.ProductName,
                        Photo = product.Photo,
                        Price = product.Price,
                        Quantity = quantity
                    });
                }
            }

            SaveCart(cart);
            return RedirectToAction("ShoppingCart");
        }

        // ==========================================
        // 4. CẬP NHẬT SỐ LƯỢNG TRONG GIỎ
        // ==========================================
        [HttpPost]
        public IActionResult UpdateCart(int productId, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductID == productId);

            if (item != null)
            {
                if (quantity > 0)
                {
                    item.Quantity = quantity; // Cập nhật số lượng mới
                }
                else
                {
                    cart.Remove(item); // Nếu số lượng <= 0 thì xóa luôn
                }
                SaveCart(cart);
            }
            return RedirectToAction("ShoppingCart");
        }

        // ==========================================
        // 5. XÓA 1 SẢN PHẨM KHỎI GIỎ VÀ XÓA SẠCH GIỎ
        // ==========================================
        public IActionResult RemoveFromCart(int id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.ProductID == id);
            if (item != null)
            {
                cart.Remove(item);
                SaveCart(cart);
            }
            return RedirectToAction("ShoppingCart");
        }

        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove(SHOPPING_CART);
            return RedirectToAction("ShoppingCart");
        }

        // ==========================================
        // 6. HIỂN THỊ TRANG THANH TOÁN (Form điền thông tin)
        // ==========================================
        [HttpGet]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public IActionResult Checkout()
        {
            var cart = GetCart();
            if (cart == null || cart.Count == 0) return RedirectToAction("ShoppingCart");

            // Gọi hàm ListOfProvinces() từ CommonDataService và gán vào ViewBag
            ViewBag.Provinces = SV22T1020375.BusinessLayers.CommonDataService.ListOfProvinces();

            // Trả về view Checkout.cshtml kèm danh sách giỏ hàng để khách xem lại
            return View(cart);
        }

        // ==========================================
        // 7. XỬ LÝ THÔNG TIN FORM THANH TOÁN
        // ==========================================
        [HttpPost]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public IActionResult ProcessCheckout(string deliveryProvince, string deliveryAddress, string paymentMethod)
        {
            // Lưu tạm thông tin giao hàng vào Session để không bị mất khi chuyển trang
            HttpContext.Session.SetString("DELIVERY_PROVINCE", deliveryProvince ?? "");
            HttpContext.Session.SetString("DELIVERY_ADDRESS", deliveryAddress ?? "");
            HttpContext.Session.SetString("PAYMENT_METHOD", paymentMethod ?? "Direct");

            if (paymentMethod == "VNPay")
            {
                // Nếu chọn ví điện tử, chuyển sang trang quét mã mô phỏng
                return RedirectToAction("VNPayMock");
            }

            // Nếu thanh toán tiền mặt (Direct), bỏ qua bước quét mã, tạo đơn luôn
            return RedirectToAction("CreateOrder");
        }

        // ==========================================
        // 8. TRANG MÔ PHỎNG THANH TOÁN VNPAY
        // ==========================================
        [HttpGet]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public IActionResult VNPayMock()
        {
            return View(); // Trả về giao diện giả lập VNPay
        }

        // ==========================================
        // 9. TẠO ĐƠN HÀNG VÀ LƯU VÀO DATABASE
        // ==========================================
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> CreateOrder()
        {
            var cart = GetCart();
            if (cart == null || cart.Count == 0) return RedirectToAction("ShoppingCart");

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int customerId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy lại thông tin đã lưu ở bước 7
            string province = HttpContext.Session.GetString("DELIVERY_PROVINCE") ?? "";
            string address = HttpContext.Session.GetString("DELIVERY_ADDRESS") ?? "";
            string paymentMethod = HttpContext.Session.GetString("PAYMENT_METHOD") ?? "Direct";

            // MẸO: Đánh dấu đơn đã thanh toán vào địa chỉ giao hàng nếu dùng VNPay
            if (paymentMethod == "VNPay")
            {
                address = "[Đã thanh toán VNPay] " + address;
            }

            // Tạo đơn hàng mới
            var order = new SV22T1020375.Models.Sales.Order
            {
                CustomerID = customerId,
                DeliveryProvince = province,
                DeliveryAddress = address
            };

            int orderId = await OrderDataService.CreateOrderAsync(order);

            // Lưu chi tiết các mặt hàng vào đơn
            foreach (var item in cart)
            {
                var detail = new SV22T1020375.Models.Sales.OrderDetail
                {
                    OrderID = orderId,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    SalePrice = item.Price
                };
                await OrderDataService.AddOrderDetailAsync(detail);
            }

            // Xóa giỏ hàng và dọn dẹp bộ nhớ tạm
            HttpContext.Session.Remove(SHOPPING_CART);
            HttpContext.Session.Remove("DELIVERY_PROVINCE");
            HttpContext.Session.Remove("DELIVERY_ADDRESS");
            HttpContext.Session.Remove("PAYMENT_METHOD");

            // Tạo xong thì chuyển hướng sang trang Danh sách Đơn hàng của khách
            return RedirectToAction("Index", "Order");
        }
    }
}