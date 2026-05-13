document.addEventListener('DOMContentLoaded', () => {
    // Mobile menu toggle
    const hamburger = document.getElementById('hamburger');
    const navLinks = document.getElementById('navLinks');
    hamburger?.addEventListener('click', () => {
        navLinks.classList.toggle('open');
        const spans = hamburger.querySelectorAll('span');
        if (navLinks.classList.contains('open')) {
            spans[0].style.transform = 'rotate(45deg) translate(5px, 5px)';
            spans[1].style.opacity = '0';
            spans[2].style.transform = 'rotate(-45deg) translate(5px, -5px)';
        } else {
            spans[0].style.transform = '';
            spans[1].style.opacity = '';
            spans[2].style.transform = '';
        }
    });

    // Close menu on link click
    navLinks?.querySelectorAll('a').forEach(link => {
        link.addEventListener('click', () => {
            navLinks.classList.remove('open');
            const spans = hamburger.querySelectorAll('span');
            spans[0].style.transform = '';
            spans[1].style.opacity = '';
            spans[2].style.transform = '';
        });
    });

    // Menu filter
    const filterBtns = document.querySelectorAll('.filter-btn');
    const menuCards = document.querySelectorAll('.menu-card');
    filterBtns.forEach(btn => {
        btn.addEventListener('click', () => {
            filterBtns.forEach(b => b.classList.remove('active'));
            btn.classList.add('active');
            const cat = btn.dataset.cat;
            menuCards.forEach(card => {
                if (cat === 'all' || card.dataset.cat === cat) {
                    card.style.display = '';
                    card.style.animation = 'fadeUp .4s ease';
                } else {
                    card.style.display = 'none';
                }
            });
        });
    });

    // Navbar scroll effect
    const nav = document.querySelector('.nav');
    window.addEventListener('scroll', () => {
        nav.style.borderBottomColor = window.scrollY > 50
            ? 'rgba(255,140,0,0.15)'
            : 'rgba(255,255,255,0.06)';
    });

    // Animate stats on scroll
    const statEls = document.querySelectorAll('.stat-val');
    const observed = new Set();
    const observer = new IntersectionObserver(entries => {
        entries.forEach(entry => {
            if (entry.isIntersecting && !observed.has(entry.target)) {
                observed.add(entry.target);
                animateNumber(entry.target);
            }
        });
    }, { threshold: 0.5 });
    statEls.forEach(el => observer.observe(el));

    function animateNumber(el) {
        const raw = el.dataset.val;
        const isDecimal = raw.includes('.');
        const target = parseFloat(raw);
        const suffix = el.dataset.suffix || '';
        const prefix = el.dataset.prefix || '';
        const duration = 1500;
        const start = performance.now();

        function update(now) {
            const progress = Math.min((now - start) / duration, 1);
            const eased = 1 - Math.pow(1 - progress, 3);
            const current = isDecimal
                ? (target * eased).toFixed(1)
                : Math.floor(target * eased);
            el.textContent = prefix + current + suffix;
            if (progress < 1) requestAnimationFrame(update);
        }
        requestAnimationFrame(update);
    }
});

// Fade-up animation
const style = document.createElement('style');
style.textContent = `@keyframes fadeUp{from{opacity:0;transform:translateY(16px)}to{opacity:1;transform:translateY(0)}}`;
document.head.appendChild(style);
