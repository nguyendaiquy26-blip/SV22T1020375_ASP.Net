using SV22T1020375.Models.Sales;

namespace SV22T1020375.Admin
{
    /// <summary>
    /// Lớp cung cấp các chức năng xử lý trên giỏ hàng
    /// Giỏ hàng được lưu trong Session 
    /// </summary>
    public static class ShoppingCartHelper
    {
        private const string CART = "ShoppingCart";
        // Lấy giỏ hàng từ session
        public static List<OrderDetailViewInfo> GetShoppingCart()
        {
            var cart = ApplicationContext.GetSessionData <List<OrderDetailViewInfo>> (CART);
            if (cart == null)
            {
                cart = new List<OrderDetailViewInfo>();
                ApplicationContext.SetSessionData(CART, cart);
            }
            return cart;
        }
        // Lấy thông tin 1 mặt hàng từ giỏ
        public static OrderDetailViewInfo? GetCartItem(int productID)
        {
            //Tìm sản phẩm
            var cart = GetShoppingCart();
            var item = cart.Find(m => m.ProductID == productID); ;
            return item;
        }


        /// <summary>
        /// Thêm hàng vào giỏ hàng
        /// </summary>
        /// <param name="item"></param>
        public static void AddItemToCart(OrderDetailViewInfo item)
        {
            var cart = GetShoppingCart ();
            var existItem = cart.Find(m => m.ProductID == item.ProductID);
            if (existItem == null)
            {
                cart.Add(item);
            }
            else
            {
                existItem.Quantity += item.Quantity;
                existItem.SalePrice += item.SalePrice;
            }
            ApplicationContext.SetSessionData (CART, cart);

        }

        public static void UpdateCartItem(int productID, int quatity, decimal salePrice)
        {

            var cart = GetShoppingCart();
            var item = cart.Find(m => m.ProductID == productID);
            if (item != null)
            {
                item.Quantity = quatity; ;
                item.SalePrice = salePrice;
                ApplicationContext.SetSessionData (CART, cart);
            }
        }

        //xóa mặt hàng ra khỏi giỏ hàng
        public static void RemoveItemFromCart(int productID)
        {
            var cart = GetShoppingCart();
            int index = cart.FindIndex(m=>m.ProductID == productID);
            //tim thay thì lớp hơn 0

            if(index >= 0)
            {
                cart.RemoveAt(index);
                ApplicationContext.SetSessionData (CART, cart);
                //Xóa xong thì lưu lại giỏ
            }
        }

        //Xóa toàn bộ giỏ hàng 
        public static void ClearCart()
        {
            var newCart = new List<OrderDetailViewInfo>();
            ApplicationContext.SetSessionData(CART, newCart);
        }
    }
}
