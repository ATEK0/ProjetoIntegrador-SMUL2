(function () {
    function createToast(message, type) {
        var container = document.getElementById('toast-container');
        if (!container) return;

        var toast = document.createElement('div');
        toast.className = 'toast toast--' + type;
        toast.setAttribute('role', 'alert');

        var iconClass = type === 'success' ? 'fa-circle-check' : 'fa-circle-exclamation';

        toast.innerHTML =
            '<i class="fas ' + iconClass + ' toast__icon"></i>' +
            '<span class="toast__message"></span>' +
            '<button type="button" class="toast__close" aria-label="Fechar"><i class="fas fa-xmark"></i></button>' +
            '<div class="toast__progress"></div>';

        toast.querySelector('.toast__message').textContent = message;

        function removeToast() {
            if (toast.classList.contains('toast--out')) return;
            toast.classList.add('toast--out');
            setTimeout(function () { toast.remove(); }, 300);
        }

        toast.querySelector('.toast__close').addEventListener('click', removeToast);
        container.appendChild(toast);

        // Auto-dismiss after the progress bar completes (6s)
        setTimeout(removeToast, 6000);
    }

    function boot() {
        var dataEl = document.getElementById('toast-data');
        if (!dataEl) return;

        try {
            var items = JSON.parse(dataEl.textContent || '[]');
            if (!Array.isArray(items) || items.length === 0) return;

            items.forEach(function (item, index) {
                if (item && item.message) {
                    // Stagger multiple toasts for a polished feel
                    setTimeout(function () {
                        createToast(item.message, item.type === 'success' ? 'success' : 'error');
                    }, index * 120);
                }
            });
        } catch (e) {
            console.error('Toast data inválido', e);
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', boot);
    } else {
        boot();
    }
})();
