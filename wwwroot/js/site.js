// SentinelAI - site.js
// File upload preview + alert dismiss helpers

(function () {
    'use strict';

    // ── File Upload Preview ──────────────────────────────────────────────────
    var fileInput = document.getElementById('evidenceFile');
    var previewContainer = document.getElementById('filePreviewContainer');
    var previewImage = document.getElementById('filePreviewImage');
    var previewFileName = document.getElementById('filePreviewName');

    if (fileInput) {
        fileInput.addEventListener('change', function (e) {
            var file = e.target.files[0];
            if (!file) {
                if (previewContainer) previewContainer.style.display = 'none';
                return;
            }

            if (previewFileName) {
                previewFileName.textContent = file.name + ' (' + (file.size / 1024).toFixed(1) + ' KB)';
            }

            var ext = file.name.split('.').pop().toLowerCase();
            if ((ext === 'jpg' || ext === 'jpeg' || ext === 'png') && previewImage) {
                var reader = new FileReader();
                reader.onload = function (ev) {
                    previewImage.src = ev.target.result;
                    previewImage.style.display = 'block';
                };
                reader.readAsDataURL(file);
            } else {
                if (previewImage) previewImage.style.display = 'none';
            }

            if (previewContainer) previewContainer.style.display = 'block';
        });
    }

    // ── Auto-dismiss Alerts ──────────────────────────────────────────────────
    var autoDismissAlerts = document.querySelectorAll('.alert-auto-dismiss');
    autoDismissAlerts.forEach(function (alertEl) {
        setTimeout(function () {
            var bsAlert = bootstrap.Alert.getOrCreateInstance(alertEl);
            if (bsAlert) bsAlert.close();
        }, 5000);
    });

    // ── Animate stat cards on load ───────────────────────────────────────────
    var statCards = document.querySelectorAll('.stat-card, .feature-card, .awareness-card');
    if ('IntersectionObserver' in window) {
        var observer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    entry.target.classList.add('fade-in-up');
                    observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.1 });

        statCards.forEach(function (card) {
            observer.observe(card);
        });
    }

    // ── Confirm delete dialogs ───────────────────────────────────────────────
    var deleteLinks = document.querySelectorAll('[data-confirm]');
    deleteLinks.forEach(function (el) {
        el.addEventListener('click', function (e) {
            if (!confirm(el.getAttribute('data-confirm'))) {
                e.preventDefault();
            }
        });
    });

})();
