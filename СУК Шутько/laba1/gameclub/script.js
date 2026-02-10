document.addEventListener('DOMContentLoaded', () => {
    // Слайдер акций
    const slides = document.querySelectorAll('.slide');
    const prevBtn = document.querySelector('.slider-btn.prev');
    const nextBtn = document.querySelector('.slider-btn.next');
    let currentSlide = 0;

    function showSlide(index) {
        slides.forEach((slide, i) => {
            slide.classList.toggle('active', i === index);
        });
    }

    prevBtn.addEventListener('click', () => {
        currentSlide = (currentSlide - 1 + slides.length) % slides.length;
        showSlide(currentSlide);
    });

    nextBtn.addEventListener('click', () => {
        currentSlide = (currentSlide + 1) % slides.length;
        showSlide(currentSlide);
    });

    setInterval(() => {
        currentSlide = (currentSlide + 1) % slides.length;
        showSlide(currentSlide);
    }, 5000);

    // FAQ аккордеон
    const faqItems = document.querySelectorAll('.faq-item');
    faqItems.forEach(item => {
        item.addEventListener('click', () => {
            item.classList.toggle('active');
        });
    });

    // Таймер обратного отсчета для акции
    const countdown = document.getElementById('countdown');
    if (countdown) {
        const endDate = new Date('2025-09-30T23:59:59').getTime();
        const updateCountdown = () => {
            const now = new Date().getTime();
            const distance = endDate - now;
            if (distance < 0) {
                countdown.textContent = 'Акция завершена!';
                return;
            }
            const days = Math.floor(distance / (1000 * 60 * 60 * 24));
            const hours = Math.floor((distance % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
            const minutes = Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60));
            countdown.textContent = `Осталось: ${days}д ${hours}ч ${minutes}м`;
        };
        setInterval(updateCountdown, 60000);
        updateCountdown();
    }
});