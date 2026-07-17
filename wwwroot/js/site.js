// بوابة العمل - Site JS

// Auto-dismiss toasts after 5 seconds
document.addEventListener('DOMContentLoaded', () => {
    // Activate Bootstrap tooltips
    document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(el => {
        new bootstrap.Tooltip(el);
    });

    // Highlight active nav links
    const path = window.location.pathname.toLowerCase();
    document.querySelectorAll('.nav-link').forEach(link => {
        if (link.getAttribute('href') && path.startsWith(link.getAttribute('href').toLowerCase()) && link.getAttribute('href') !== '/') {
            link.classList.add('active');
        }
    });
});

// Format numbers in Arabic locale
function formatNumber(n) {
    return new Intl.NumberFormat('ar-SA').format(n);
}

// Confirm delete
function confirmDelete(message) {
    return confirm(message || 'هل أنت متأكد من الحذف؟');
}
