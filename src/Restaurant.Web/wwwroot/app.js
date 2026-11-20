// Premium menu micro-interactions: scroll reveal, gallery, AR launch.
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

    // (The hero parallax was removed: it captured the Home hero element once and kept moving it
    //  on scroll. Blazor's enhanced navigation reuses DOM elements across pages, so after
    //  Home → /menu it scaled and shifted /menu's content out of the frame.)

    // Re-run after every Blazor enhanced navigation / interactive render.
    function run() { initReveal(); }
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

// Bring the active category chip into view within the horizontal nav strip (centered).
function chipDelta(strip) {
    const el = strip.querySelector('.cat-chip.active');
    if (!el) return 0;
    const stripRect = strip.getBoundingClientRect();
    const elRect = el.getBoundingClientRect();
    return (elRect.left - stripRect.left) - (strip.clientWidth - elRect.width) / 2;
}
window.scrollChipIntoView = function () {
    const strip = document.querySelector('.cat-nav .cat-strip');
    if (!strip) return;
    const delta = chipDelta(strip);
    if (Math.abs(delta) > 1) strip.scrollBy({ left: delta, behavior: 'smooth' });
};

// Switching category replaces the whole page DOM (enhanced navigation, then again when the
// interactive circuit starts), so every new chip strip would start at scrollLeft 0 and then
// visibly race back to the active chip. Keep the strip's last position and re-apply it
// instantly to each new strip (MutationObserver runs before paint), then glide only the
// short distance to the newly active chip.
(function () {
    let lastLeft = null;
    const placed = new WeakSet();
    document.addEventListener('scroll', e => {
        const t = e.target;
        if (t instanceof Element && t.matches('.cat-nav .cat-strip')) lastLeft = t.scrollLeft;
    }, true);

    function placeStrip() {
        const strip = document.querySelector('.cat-nav .cat-strip');
        if (!strip || placed.has(strip)) return;
        placed.add(strip);
        if (lastLeft === null) {
            // First visit: jump straight to the active chip, no animation from the start.
            strip.scrollLeft += chipDelta(strip);
        } else {
            strip.scrollLeft = lastLeft;
            requestAnimationFrame(() => window.scrollChipIntoView());
        }
        lastLeft = strip.scrollLeft;
    }

    new MutationObserver(placeStrip).observe(document.documentElement, { childList: true, subtree: true });
    document.addEventListener('DOMContentLoaded', placeStrip);
    placeStrip();
})();
