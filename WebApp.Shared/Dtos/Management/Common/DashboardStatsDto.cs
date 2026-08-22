using WebApp.Shared.Dtos.CustomerDtos.Order;

namespace WebApp.Shared.Dtos.Management.Common
{
    // Component dùng: Dashboard.razor (Admin)
    public record DashboardStatsDto(
        // === KHU VỰC 1: CHỈ SỐ KPI TỔNG QUAN ===
        // 1. Tài chính
        decimal TotalRevenue,               // Tổng doanh thu thực tế
        decimal RevenueThisMonth,           // Doanh thu tháng này
        decimal RevenueGrowthPercent,       // % Tăng trưởng so với tháng trước (+15.2% hoặc -5.0%)
        decimal AverageOrderValue,          // Giá trị trung bình / đơn hàng (AOV)
        // 2. Vận hành & Đơn hàng
        int TotalOrders,                    // Tổng số đơn
        double OrderSuccessRate,            // Tỷ lệ giao thành công (%)
        int PendingOrders,                  // Đơn chờ duyệt
        int ProcessingOrders,               // Đang đóng gói
        int ShippingOrders,                 // Đang giao hàng
        int CompletedOrders,                // Giao thành công
        int CancelledOrders,                // Đơn đã hủy
        int ReturnedOrders,                 // Đơn trả hàng
        // 3. Kho bãi & Sức khỏe Tồn kho
        int TotalProducts,                  // Tổng số đầu sách (SKUs)
        int TotalStockQuantity,             // Tổng số cuốn sách trong kho
        decimal TotalInventoryValue,        // Tổng giá trị vốn hàng tồn kho
        int HealthyStockCount,              // Sách tồn kho an toàn (> 5 cuốn)
        int LowStockCount,                  // Sách sắp hết hàng (1 - 5 cuốn)
        int OutOfStockCount,                // Sách đã hết hàng (0 cuốn)
        // 4. Khách hàng & Phân khúc
        int TotalCustomers,                 // Tổng số khách hàng
        int NewCustomersThisMonth,          // Khách đăng ký mới trong tháng
        double RepeatCustomerRate,          // Tỷ lệ khách mua lại (>= 2 đơn) (%)
        int CustomersWithOrders,            // Khách đã từng mua hàng
        int CustomersWithoutOrders,         // Khách chưa mua hàng bao giờ

        // === KHU VỰC 2, 3, 4: DỮ LIỆU BIỂU ĐỒ ===
        List<MonthlyRevenueDto> MonthlyRevenues,      // Doanh thu 6 tháng gần nhất
        List<CategoryRevenueDto> CategoryRevenues,    // Phân bổ doanh thu theo Thể loại sách
        BookModelDistributionDto BookDistribution     // Phân bổ mô hình: Sách thương mại vs Sách ủng hộ/ sách 0 đồng
    );
    public record MonthlyRevenueDto(string MonthLabel, decimal Revenue, int OrderCount);
    public record CategoryRevenueDto(string CategoryName, decimal TotalRevenue, int TotalSold);
    public record BookModelDistributionDto(int CommercialBookCount, int FreeBookCount, int DonationBookCount);
}
