// Cầu nối giữa C# Blazor và Chart.js
window.DashboardChart = {
    renderBarChart: function (canvasId, labels, dataValues, labelName) {
        const ctx = document.getElementById(canvasId);
        if (!ctx) return;

        // Nếu đã có chart cũ trên canvas này thì hủy để vẽ mới (tránh đè hình)
        if (window.myChartInstance) {
            window.myChartInstance.destroy();
        }

        window.myChartInstance = new Chart(ctx, {
            type: 'bar', // Biểu đồ dạng cột (có thể đổi thành 'line' nếu muốn dạng đường)
            data: {
                labels: labels, // Trục X: danh sách Ngày/Tháng
                datasets: [{
                    label: labelName, // Tên chú thích (VD: Doanh thu)
                    data: dataValues, // Trục Y: Mức tiền/doanh thu tương ứng
                    backgroundColor: 'rgba(54, 162, 235, 0.6)',
                    borderColor: 'rgba(54, 162, 235, 1)',
                    borderWidth: 1,
                    borderRadius: 4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    y: {
                        beginAtZero: true
                    }
                }
            }
        });
    }
};