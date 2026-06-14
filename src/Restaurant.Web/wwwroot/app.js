// Premium menu micro-interactions: scroll reveal, hero parallax, gallery, AR launch.
(function () {
    function initReveal() {
        const els = document.querySelectorAll('.reveal:not(.in)');
        if (!('IntersectionObserver' in window)) {
            els.forEach(e => e.classList.add('in'));
            return;
        }
        const io = new IntersectionObserver((entries) => {
            entries.forEach(en => {
                if (en.isIntersecting) { en.target.classList.add('in'); io.unobserve(en.target); }
            });
        }, { threshold: 0.12 });
        els.forEach(e => io.observe(e));
    }

    function initParallax() {
        const bg = document.querySelector('.hero-bg');
        if (!bg) return;
        let ticking = false;
        window.addEventListener('scroll', () => {
            if (ticking) return;
            ticking = true;
            requestAnimationFrame(() => {
                const y = window.scrollY * 0.4;
                bg.style.transform = `scale(1.12) translateY(${y}px)`;
                ticking = false;
            });
        }, { passive: true });
    }

    // Re-run after every Blazor enhanced navigation / interactive render.
    function run() { initReveal(); initParallax(); }
    document.addEventListener('DOMContentLoaded', run);
    if (window.Blazor) {
        Blazor.addEventListener?.('enhancedload', run);
    }
    document.addEventListener('enhancedload', run);
    // Fallback: observe DOM mutations to catch interactive renders.
    new MutationObserver(() => initReveal()).observe(document.documentElement, { childList: true, subtree: true });
    run();
})();

// Gallery thumbnail switching via event delegation (works in static SSR).
document.addEventListener('click', function (e) {
    const thumb = e.target.closest('.gallery-thumbs img');
    if (!thumb) return;
    const main = document.getElementById('gallery-main-img');
    if (main) main.src = thumb.dataset.full || thumb.src;
    document.querySelectorAll('.gallery-thumbs img').forEach(t => t.classList.toggle('active', t === thumb));
});

// Launch AR on the model-viewer element.
window.launchAr = function () {
    const mv = document.querySelector('model-viewer');
    if (mv && typeof mv.activateAR === 'function') mv.activateAR();
};

// Smooth-scroll to a category section (offset handled by the section's scroll-margin-top).
window.scrollToCat = function (slug) {
    const el = document.getElementById('cat-' + slug);
    if (el) el.scrollIntoView({ behavior: 'smooth', block: 'start' });
};
