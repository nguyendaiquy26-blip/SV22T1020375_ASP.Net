using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020375.BusinessLayers;
using SV22T1020375.Models.Catalog;
using SV22T1020375.Models.Sales;
using System.Threading.Tasks;
using System;

namespace SV22T1020375.Admin.Controllers
{
    [Authorize(Roles = WebUserRoles.Sales + "," + WebUserRoles.Administrator)]
    /// <summary>
    /// Thực hiện các chức năng quản lý và xử lý đơn hàng
    /// </summary>
    public class OrderController : Controller
    {
        private const string SEARCH_PRODUCT = "SearchProductToSale";

        private int GetCurrentEmployeeID()
        {
            int employeeID = 0;
            int.TryParse(User.FindFirst("UserId")?.Value, out employeeID);
            return employeeID;
        }

        public async Task<IActionResult> Index()
        {
            var input = ApplicationContext.GetSessionData<OrderSearchInput>("OrderSearchInput");
            if (input == null)
                input = new OrderSearchInput()
                {
                    Page = 1,
                    PageSize = ApplicationContext.PageSize,
                    SearchValue = "",
                    Status = 0
                };

            ViewBag.Title = "Quản lý đơn hàng";
            ViewBag.OrderStatuses = await OrderDataService.ListOrderStatusesAsync();
            return View(input);
        }

        public async Task<IActionResult> Search(OrderSearchInput input)
        {
            if (input.Page < 1) input.Page = 1;
            if (input.PageSize <= 0) input.PageSize = ApplicationContext.PageSize;

            input.SearchValue ??= "";

            var result = await OrderDataService.ListOrdersAsync(input);
            ApplicationContext.SetSessionData("OrderSearchInput", input);

            return PartialView(result);
        }

        public IActionResult Create()
        {
            ViewBag.Title = "Lập đơn hàng";

            // SỬ DỤNG ĐÚNG HÀM TỪ FILE CỦA BẠN: CustomerDataService.ListCustomers()
            ViewBag.Customers = CustomerDataService.ListCustomers();

            var input = ApplicationContext.GetSessionData<ProductSearchInput>(SEARCH_PRODUCT);
            if (input == null)
                input = new ProductSearchInput()
                {
                    Page = 1,
                    PageSize = 3,
                    SearchValue = "",
                    CategoryID = 0,
                    SupplierID = 0,
                    MinPrice = 0,
                    MaxPrice = 0,
                };

            return View(input);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(int customerID = 0, string? province = "", string? address = "")
        {
            province = province?.Trim() ?? "";
            address = address?.Trim() ?? "";

            var cart = ShoppingCartHelper.GetShoppingCart();
            if (cart.Count == 0) return Json(new ApiResult(0, "Giỏ hàng đang trống"));
            if (string.IsNullOrWhiteSpace(province)) return Json(new ApiResult(0, "Vui lòng chọn tỉnh/thành giao hàng"));
            if (string.IsNullOrWhiteSpace(address)) return Json(new ApiResult(0, "Vui lòng nhập địa chỉ giao hàng"));
            if (customerID <= 0) return Json(new ApiResult(0, "Vui lòng chọn khách hàng")); // Bổ sung kiểm tra khách hàng

            var newOrder = new Order()
            {
                CustomerID = customerID,
                DeliveryProvince = province,
                DeliveryAddress = address,
                EmployeeID = GetCurrentEmployeeID(),
                OrderTime = DateTime.Now
            };

            int orderID = await OrderDataService.CreateOrderAsync(newOrder);
            if (orderID <= 0) return Json(new ApiResult(0, "Không thể lập đơn hàng"));

            foreach (var item in cart)
            {
                var newDetail = new OrderDetail()
                {
                    OrderID = orderID,
                    ProductID = item.ProductID,
                    Quantity = item.Quantity,
                    SalePrice = item.SalePrice
                };

                bool result = await OrderDataService.AddOrderDetailAsync(newDetail);
                if (!result) return Json(new ApiResult(0, $"Không thể thêm mặt hàng {item.ProductID} vào đơn hàng"));
            }
            ShoppingCartHelper.ClearCart();
            return Json(new ApiResult(orderID, ""));
        }

        public async Task<IActionResult> SearchProduct(ProductSearchInput input)
        {
            var result = await CatalogDataService.ListProductsAsync(input);
            ApplicationContext.SetSessionData(SEARCH_PRODUCT, input);
            return PartialView(result);
        }

        public IActionResult ShowCart()
        {
            var cart = ShoppingCartHelper.GetShoppingCart();
            return PartialView(cart);
        }

        public async Task<IActionResult> Action(int id)
        {
            ViewBag.Title = "Xử lý đơn hàng";
            var data = await OrderDataService.GetOrderAsync(id);
            if (data == null) return RedirectToAction("Index");

            return View(data);
        }

        public async Task<IActionResult> Detail(int id)
        {
            ViewBag.Title = "Chi tiết đơn hàng";
            var order = await OrderDataService.GetOrderAsync(id);
            if (order == null) return RedirectToAction("Index");

            ViewBag.OrderDetails = await OrderDataService.ListOrderDetailsAsync(id);
            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> AddCartItem(int productId = 0, int quantity = 0, decimal price = 0)
        {
            if (productId <= 0) return Json(new ApiResult(0, "Mã mặt hàng không hợp lệ"));
            if (quantity <= 0) return Json(new ApiResult(0, "Số lượng phải lớn hơn 0"));
            if (price <= 0) return Json(new ApiResult(0, "Giá bán không hợp lệ"));

            var product = await CatalogDataService.GetProductAsync(productId);
            if (product == null) return Json(new ApiResult(0, "Mặt hàng không tồn tại"));

            if (product.IsSelling != true) return Json(new ApiResult(0, "Mặt hàng này đã ngưng bán"));

            var item = new OrderDetailViewInfo()
            {
                ProductID = productId,
                ProductName = product.ProductName ?? "",
                Unit = product.Unit ?? "",
                Photo = product.Photo ?? "nophoto.png",
                Quantity = quantity,
                SalePrice = price
            };
            ShoppingCartHelper.AddItemToCart(item);
            return Json(new ApiResult(1, ""));
        }

        [HttpGet]
        public IActionResult EditCartItem(int productId = 0)
        {
            if (productId <= 0) return Content("Mã mặt hàng không hợp lệ");
            var item = ShoppingCartHelper.GetCartItem(productId);
            if (item == null) return Content("Không tìm thấy mặt hàng trong giỏ hàng");

            return PartialView(item);
        }

        [HttpPost]
        public IActionResult UpdateCartItem(int productID, int quantity, decimal salePrice)
        {
            if (productID <= 0) return Json(new ApiResult(0, "Mã mặt hàng không hợp lệ"));
            if (quantity <= 0) return Json(new ApiResult(0, "Số lượng phải lớn hơn 0"));
            if (salePrice <= 0) return Json(new ApiResult(0, "Giá bán phải lớn hơn 0"));

            var item = ShoppingCartHelper.GetCartItem(productID);
            if (item == null) return Json(new ApiResult(0, "Không tìm thấy mặt hàng trong giỏ hàng"));

            ShoppingCartHelper.UpdateCartItem(productID, quantity, salePrice);
            return Json(new ApiResult(1, ""));
        }

        public IActionResult DeleteCartItem(int productId = 0)
        {
            if (productId <= 0) return Content("Mã mặt hàng không hợp lệ");
            if (Request.Method == "POST")
            {
                ShoppingCartHelper.RemoveItemFromCart(productId);
                return Json(new ApiResult(1, ""));
            }
            ViewBag.ProductID = productId;
            return PartialView();
        }

        public IActionResult ClearCart()
        {
            if (Request.Method == "POST")
            {
                ShoppingCartHelper.ClearCart();
                return Json(new ApiResult(1, ""));
            }
            return PartialView();
        }

        [HttpGet]
        public async Task<IActionResult> EditDetail(int orderID = 0, int productID = 0)
        {
            if (orderID <= 0 || productID <= 0) return Content("Tham số không hợp lệ");
            var item = await OrderDataService.GetOrderDetailAsync(orderID, productID);
            if (item == null) return Content("Không tìm thấy mặt hàng trong đơn hàng");

            return PartialView(item);
        }

        [HttpPost]
        public async Task<IActionResult> SaveDetail(int orderID, int productID, int quantity, decimal salePrice)
        {
            if (orderID <= 0 || productID <= 0) return Json(new ApiResult(0, "Tham số không hợp lệ"));
            if (quantity <= 0) return Json(new ApiResult(0, "Số lượng phải lớn hơn 0"));
            if (salePrice <= 0) return Json(new ApiResult(0, "Đơn giá phải lớn hơn 0"));

            var data = new OrderDetail()
            {
                OrderID = orderID,
                ProductID = productID,
                Quantity = quantity,
                SalePrice = salePrice
            };

            bool result = await OrderDataService.UpdateOrderDetailAsync(data);
            if (!result) return Json(new ApiResult(0, "Không thể cập nhật mặt hàng. Chỉ được sửa khi đơn hàng ở trạng thái New."));

            return Json(new ApiResult(1, ""));
        }

        public async Task<IActionResult> DeleteDetail(int orderID = 0, int productID = 0)
        {
            if (orderID <= 0 || productID <= 0) return Content("Tham số không hợp lệ");

            if (Request.Method == "POST")
            {
                bool result = await OrderDataService.DeleteOrderDetailAsync(orderID, productID);
                if (!result) return Content("Không thể xóa mặt hàng. Chỉ được xóa khi đơn hàng ở trạng thái New.");

                return RedirectToAction("Detail", new { id = orderID });
            }

            var item = await OrderDataService.GetOrderDetailAsync(orderID, productID);
            if (item == null) return Content("Không tìm thấy mặt hàng trong đơn hàng");

            return PartialView(item);
        }

        [HttpGet]
        public async Task<IActionResult> Accept(int id)
        {
            ViewBag.Title = "Duyệt đơn hàng";
            var data = await OrderDataService.GetOrderAsync(id);
            if (data == null) return Content("Không tìm thấy đơn hàng");
            return PartialView(data);
        }

        [HttpPost]
        public async Task<IActionResult> Accept(int id, string submit = "")
        {
            bool result = await OrderDataService.AcceptOrderAsync(id, GetCurrentEmployeeID());
            if (!result) return Content("Không thể duyệt đơn hàng");
            return RedirectToAction("Detail", new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Shipping(int id)
        {
            ViewBag.Title = "Chuyển giao hàng";
            var data = await OrderDataService.GetOrderAsync(id);
            if (data == null) return Content("Không tìm thấy đơn hàng");
            return PartialView(data);
        }

        [HttpPost]
        public async Task<IActionResult> Shipping(int id, int shipperID)
        {
            if (shipperID <= 0) return Content("Vui lòng chọn người giao hàng");
            bool result = await OrderDataService.ShippingOrderAsync(id, shipperID);
            if (!result) return Content("Không thể chuyển giao hàng");
            return RedirectToAction("Detail", new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Finish(int id)
        {
            ViewBag.Title = "Hoàn tất đơn hàng";
            var data = await OrderDataService.GetOrderAsync(id);
            if (data == null) return Content("Không tìm thấy đơn hàng");
            return PartialView(data);
        }

        [HttpPost]
        public async Task<IActionResult> Finish(int id, string submit = "")
        {
            bool result = await OrderDataService.FinishOrderAsync(id);
            if (!result) return Content("Không thể hoàn tất đơn hàng");
            return RedirectToAction("Detail", new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Reject(int id)
        {
            ViewBag.Title = "Từ chối đơn hàng";
            var data = await OrderDataService.GetOrderAsync(id);
            if (data == null) return Content("Không tìm thấy đơn hàng");
            return PartialView(data);
        }

        [HttpPost]
        public async Task<IActionResult> Reject(int id, string submit = "")
        {
            bool result = await OrderDataService.RejectOrderAsync(id);
            if (!result) return Content("Không thể từ chối đơn hàng");
            return RedirectToAction("Detail", new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Cancel(int id)
        {
            ViewBag.Title = "Hủy đơn hàng";
            var data = await OrderDataService.GetOrderAsync(id);
            if (data == null) return Content("Không tìm thấy đơn hàng");
            return PartialView(data);
        }

        [HttpPost]
        public async Task<IActionResult> Cancel(int id, string submit = "")
        {
            bool result = await OrderDataService.CancelOrderAsync(id);
            if (!result) return Content("Không thể hủy đơn hàng");
            return RedirectToAction("Detail", new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            ViewBag.Title = "Xóa đơn hàng";
            var data = await OrderDataService.GetOrderAsync(id);
            if (data == null) return Content("Không tìm thấy đơn hàng");
            return PartialView(data);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteData(int id)
        {
            bool result = await OrderDataService.DeleteOrderAsync(id);
            if (!result) return Content("Không thể xóa đơn hàng. Chỉ được xóa khi đơn hàng ở trạng thái New.");
            return RedirectToAction("Index");
        }
    }
}