using SV22T1020375.BusinessLayers;
using SV22T1020375.DataLayers.Interfaces;
using SV22T1020375.DataLayers.SQLServer;
using SV22T1020375.Models.Common;
using SV22T1020375.Models.Sales;

// Đã sửa lại namespace cho đồng bộ với các project khác
namespace SV22T1020375.BusinessLayers
{
    /// <summary>
    /// Cung cấp các chức năng xử lý dữ liệu liên quan đến bán hàng
    /// bao gồm: đơn hàng (Order) và chi tiết đơn hàng (OrderDetail).
    /// </summary>
    public static class SalesDataService
    {
        private static readonly IOrderRepository orderDB;

        /// <summary>
        /// Constructor
        /// </summary>
        static SalesDataService()
        {
            orderDB = new OrderRepository(Configuration.ConnectionString);
        }

        #region Order

        /// <summary>
        /// Tìm kiếm và lấy danh sách đơn hàng dưới dạng phân trang
        /// </summary>
        public static async Task<PagedResult<OrderViewInfo>> ListOrdersAsync(OrderSearchInput input)
        {
            return await orderDB.ListAsync(input);
        }

        /// <summary>
        /// Lấy thông tin chi tiết của một đơn hàng
        /// </summary>
        public static async Task<OrderViewInfo?> GetOrderAsync(int orderID)
        {
            return await orderDB.GetAsync(orderID);
        }

        /// <summary>
        /// Tạo đơn hàng mới
        /// </summary>
        public static async Task<int> AddOrderAsync(Order data)
        {
            data.Status = OrderStatusEnum.New;
            data.OrderTime = DateTime.Now;

            return await orderDB.AddAsync(data);
        }

        /// <summary>
        /// Cập nhật thông tin đơn hàng
        /// </summary>
        public static async Task<bool> UpdateOrderAsync(Order data)
        {
            // Đã xử lý TODO: Chỉ cho cập nhật thông tin khi đơn hàng Mới hoặc Đã duyệt
            var existingOrder = await orderDB.GetAsync(data.OrderID);
            if (existingOrder == null) return false;

            if (existingOrder.Status != OrderStatusEnum.New && existingOrder.Status != OrderStatusEnum.Accepted)
            {
                return false; // Không được phép cập nhật
            }

            return await orderDB.UpdateAsync(data);
        }

        /// <summary>
        /// Xóa đơn hàng
        /// </summary>
        public static async Task<bool> DeleteOrderAsync(int orderID)
        {
            // Đã xử lý TODO: Chỉ xóa đơn hàng nếu nó Mới, Bị từ chối hoặc Đã hủy
            var existingOrder = await orderDB.GetAsync(orderID);
            if (existingOrder == null) return false;

            if (existingOrder.Status == OrderStatusEnum.Shipping || existingOrder.Status == OrderStatusEnum.Completed || existingOrder.Status == OrderStatusEnum.Accepted)
            {
                return false; // Đang giao, đã hoàn tất, hoặc đang xử lý thì không được xóa
            }

            return await orderDB.DeleteAsync(orderID);
        }

        #endregion

        #region Order Status Processing

        // (Phần này bạn làm rất tốt, các logic chuyển trạng thái đã chính xác, tôi giữ nguyên)

        public static async Task<bool> AcceptOrderAsync(int orderID, int employeeID)
        {
            var order = await orderDB.GetAsync(orderID);
            if (order == null) return false;

            if (order.Status != OrderStatusEnum.New) return false;

            order.EmployeeID = employeeID;
            order.AcceptTime = DateTime.Now;
            order.Status = OrderStatusEnum.Accepted;

            return await orderDB.UpdateAsync(order);
        }

        public static async Task<bool> RejectOrderAsync(int orderID, int employeeID)
        {
            var order = await orderDB.GetAsync(orderID);
            if (order == null) return false;

            if (order.Status != OrderStatusEnum.New) return false;

            order.EmployeeID = employeeID;
            order.FinishedTime = DateTime.Now;
            order.Status = OrderStatusEnum.Rejected;

            return await orderDB.UpdateAsync(order);
        }

        public static async Task<bool> CancelOrderAsync(int orderID)
        {
            var order = await orderDB.GetAsync(orderID);
            if (order == null) return false;

            if (order.Status != OrderStatusEnum.New && order.Status != OrderStatusEnum.Accepted) return false;

            order.FinishedTime = DateTime.Now;
            order.Status = OrderStatusEnum.Cancelled;

            return await orderDB.UpdateAsync(order);
        }

        public static async Task<bool> ShipOrderAsync(int orderID, int shipperID)
        {
            var order = await orderDB.GetAsync(orderID);
            if (order == null) return false;

            if (order.Status != OrderStatusEnum.Accepted) return false;

            order.ShipperID = shipperID;
            order.ShippedTime = DateTime.Now;
            order.Status = OrderStatusEnum.Shipping;

            return await orderDB.UpdateAsync(order);
        }

        public static async Task<bool> CompleteOrderAsync(int orderID)
        {
            var order = await orderDB.GetAsync(orderID);
            if (order == null) return false;

            if (order.Status != OrderStatusEnum.Shipping) return false;

            order.FinishedTime = DateTime.Now;
            order.Status = OrderStatusEnum.Completed;

            return await orderDB.UpdateAsync(order);
        }

        #endregion

        #region Order Detail

        public static async Task<List<OrderDetailViewInfo>> ListDetailsAsync(int orderID)
        {
            return await orderDB.ListDetailsAsync(orderID);
        }

        public static async Task<OrderDetailViewInfo?> GetDetailAsync(int orderID, int productID)
        {
            return await orderDB.GetDetailAsync(orderID, productID);
        }

        /// <summary>
        /// Thêm mặt hàng vào đơn hàng
        /// </summary>
        public static async Task<bool> AddDetailAsync(OrderDetail data)
        {
            // Đã xử lý TODO: Kiểm tra trạng thái đơn hàng
            var order = await orderDB.GetAsync(data.OrderID);
            if (order == null) return false;

            // Chỉ thêm sản phẩm khi đơn hàng Mới hoặc Đã Duyệt
            if (order.Status != OrderStatusEnum.New && order.Status != OrderStatusEnum.Accepted)
                return false;

            return await orderDB.AddDetailAsync(data);
        }

        /// <summary>
        /// Cập nhật mặt hàng trong đơn hàng
        /// </summary>
        public static async Task<bool> UpdateDetailAsync(OrderDetail data)
        {
            // Đã xử lý TODO: Kiểm tra trạng thái đơn hàng
            var order = await orderDB.GetAsync(data.OrderID);
            if (order == null) return false;

            if (order.Status != OrderStatusEnum.New && order.Status != OrderStatusEnum.Accepted)
                return false;

            return await orderDB.UpdateDetailAsync(data);
        }

        /// <summary>
        /// Xóa mặt hàng khỏi đơn hàng
        /// </summary>
        public static async Task<bool> DeleteDetailAsync(int orderID, int productID)
        {
            // Đã xử lý TODO: Kiểm tra trạng thái đơn hàng
            var order = await orderDB.GetAsync(orderID);
            if (order == null) return false;

            if (order.Status != OrderStatusEnum.New && order.Status != OrderStatusEnum.Accepted)
                return false;

            return await orderDB.DeleteDetailAsync(orderID, productID);
        }

        #endregion
    }
}