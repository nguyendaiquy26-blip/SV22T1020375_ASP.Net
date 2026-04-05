using Microsoft.AspNetCore.Mvc;
using SV22T1020375.BusinessLayers;
using SV22T1020375.Shop.Extensions;
using SV22T1020375.Shop.Models;

namespace SV22T1020375.Shop.Controllers
{
    public class CartController : Controller
    {
        private const string SHOPPING_CART = "ShoppingCart";

        public IActionResult Index()
        {
            var cart = HttpContext.Session.Get<List<CartItem>>(SHOPPING_CART) ?? new List<CartItem>();
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var cart = HttpContext.Session.Get<List<CartItem>>(SHOPPING_CART) ?? new List<CartItem>();

            var item = cart.FirstOrDefault(c => c.ProductID == productId);
            if (item != null)
            {
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
                        Photo = string.IsNullOrWhiteSpace(product.Photo) ? "macbook.png" : product.Photo,
                        Quantity = quantity,
                        SalePrice = product.Price
                    });
                }
            }

            HttpContext.Session.Set(SHOPPING_CART, cart);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateCart(int productId, int quantity)
        {
            var cart = HttpContext.Session.Get<List<CartItem>>(SHOPPING_CART) ?? new List<CartItem>();
            var item = cart.FirstOrDefault(c => c.ProductID == productId);
            if (item != null)
            {
                if (quantity > 0)
                    item.Quantity = quantity;
                else
                    cart.Remove(item);
            }
            HttpContext.Session.Set(SHOPPING_CART, cart);
            return RedirectToAction("Index");
        }

        public IActionResult RemoveFromCart(int id)
        {
            var cart = HttpContext.Session.Get<List<CartItem>>(SHOPPING_CART) ?? new List<CartItem>();
            var item = cart.FirstOrDefault(c => c.ProductID == id);
            if (item != null)
            {
                cart.Remove(item);
                HttpContext.Session.Set(SHOPPING_CART, cart);
            }
            return RedirectToAction("Index");
        }

        public IActionResult ClearCart()
        {
            HttpContext.Session.Remove(SHOPPING_CART);
            return RedirectToAction("Index");
        }
    }
}
