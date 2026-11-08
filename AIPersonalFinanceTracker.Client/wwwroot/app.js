window.downloadFile = (fileName, content) => {
    const blob = new Blob([content], { type: "text/csv;charset=utf-8" });
    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.href = url;
    link.download = fileName;
    link.click();
    URL.revokeObjectURL(url);
};
window.setTheme = (theme) => document.documentElement.setAttribute("data-theme", theme);
window.renderSpendingChart = (canvasId, labels, values) => {
    const canvas = document.getElementById(canvasId);
    if (!canvas || !window.Chart) return;
    const existing = window.financeCharts?.[canvasId];
    if (existing) existing.destroy();
    window.financeCharts ??= {};
    window.financeCharts[canvasId] = new Chart(canvas, {
        type: "doughnut",
        data: {
            labels,
            datasets: [{
                data: values,
                backgroundColor: ["#0d6efd", "#20c997", "#ffc107", "#dc3545", "#6f42c1", "#6c757d"],
                borderWidth: 2,
                borderColor: "#ffffff"
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: { legend: { position: "bottom" } }
        }
    });
};
