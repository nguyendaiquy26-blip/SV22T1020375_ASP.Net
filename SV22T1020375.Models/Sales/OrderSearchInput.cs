using SV22T1020375.Models.Common;
using System;

namespace SV22T1020375.Models.Sales
{
    /// <summary>
    /// Đầu vào tìm kiếm, phân trang đơn hàng
    /// </summary>
    public class OrderSearchInput : PaginationSearchInput
    {
        /// <summary>
        /// Trạng thái đơn hàng (ĐÃ SỬA THÀNH Kiểu int)
        /// </summary>
        public int Status { get; set; } = 0;

        /// <summary>
        /// Từ ngày (ngày lập đơn hàng) - Dùng string để tránh lỗi Model Binding
        /// </summary>
        public string DateFrom { get; set; } = "";

        /// <summary>
        /// Đến ngày (ngày lập đơn hàng) - Dùng string để tránh lỗi Model Binding
        /// </summary>
        public string DateTo { get; set; } = "";
    }
}