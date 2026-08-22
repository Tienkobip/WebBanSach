// JavaScript for Dashboard component

// Quản lý các instance biểu đồ để tự hủy và vẽ mới khi đổi Tab
const chartInstances = {};

function destroyChart(canvasId) {
    if (chartInstances[canvasId]) {
        chartInstances[canvasId].destroy();
        delete chartInstances[canvasId];
    }
}

// 1. BIỂU ĐỒ DOANH THU (Tab Tài chính)
export function renderRevenueChart(labels, values) {
    const ctx = document.getElementById('revenueChart');
    if (!ctx) return;
    destroyChart('revenueChart');

    chartInstances['revenueChart'] = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: 'Doanh thu',
                data: values,
                backgroundColor: 'rgba(201, 33, 39, 0.75)', /* Đỏ Fahasa */
                borderColor: '#c92127',
                borderWidth: 1.5,
                borderRadius: 6
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: { legend: { display: false } },
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        callback: function (v) { return (v / 1000000).toLocaleString() + ' Tr'; }
                    }
                }
            }
        }
    });
}

// 2. BIỂU ĐỒ TRẠNG THÁI ĐƠN HÀNG (Tab Đơn hàng)
export function renderOrderStatusChart(completed, processing, shipping, cancelled) {
    const ctx = document.getElementById('orderStatusChart');
    if (!ctx) return;
    destroyChart('orderStatusChart');

    chartInstances['orderStatusChart'] = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: ['Giao thành công', 'Chờ xử lý / Đóng gói', 'Đang giao hàng', 'Đã hủy / Trả hàng'],
            datasets: [{
                data: [completed, processing, shipping, cancelled],
                backgroundColor: ['#16a34a', '#f59e0b', '#0284c7', '#dc2626'],
                borderWidth: 2
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: { legend: { position: 'bottom' } }
        }
    });
}

// 3. BIỂU ĐỒ THỂ LOẠI SÁCH (Tab Kho bãi)
export function renderCategoryChart(labels, values) {
    const ctx = document.getElementById('categoryChart');
    if (!ctx) return;
    destroyChart('categoryChart');

    chartInstances['categoryChart'] = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: labels.length ? labels : ['Chưa có dữ liệu'],
            datasets: [{
                data: values.length ? values : [1],
                backgroundColor: ['#c92127', '#0284c7', '#16a34a', '#f59e0b', '#9333ea'],
                borderWidth: 2
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: { legend: { position: 'bottom' } }
        }
    });
}

// 4. BIỂU ĐỒ PHÂN KHÚC KHÁCH HÀNG (Tab Khách hàng)
export function renderCustomerChart(withOrders, withoutOrders) {
    const ctx = document.getElementById('customerChart');
    if (!ctx) return;
    destroyChart('customerChart');

    chartInstances['customerChart'] = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: ['Đã phát sinh đơn hàng', 'Chưa mua hàng lần nào'],
            datasets: [{
                data: [withOrders, withoutOrders],
                backgroundColor: ['#9333ea', '#9ca3af'],
                borderWidth: 2
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: { legend: { position: 'bottom' } }
        }
    });
}