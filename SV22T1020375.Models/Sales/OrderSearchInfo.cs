namespace SV22T1020375.Models.Sales
{
    /// <summary>
    /// Thông tin đơn hàng khi hiển thị trong danh sách tìm kiếm (DTO)
    /// </summary>
    public class OrderSearchInfo : Order
    {
        public string CustomerName { get; set; } = "";
        public string CustomerContactName { get; set; } = "";
        public string CustomerPhone { get; set; } = "";
        public string CustomerEmail { get; set; } = "";
        public string CustomerAddress { get; set; } = "";
        public string EmployeeName { get; set; } = "";
        public string ShipperName { get; set; } = "";
        public string ShipperPhone { get; set; } = "";
        public decimal SumOfPrice { get; set; }
    }
}