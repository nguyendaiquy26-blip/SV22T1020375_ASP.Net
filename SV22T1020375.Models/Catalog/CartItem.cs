namespace SV22T1020375.Models
{
    public class CartItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; } = "";
        public string Photo { get; set; } = "";
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        // Tự động tính thành tiền = Số lượng x Đơn giá
        public decimal TotalPrice => Quantity * Price;
    }
}