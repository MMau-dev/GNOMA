
document.addEventListener("DOMContentLoaded", () => {
    AOS.init({
        offset: 80,
        duration: 800,
        easing: "ease-out-cubic",
        once: true,
        mirror: false,
        disable: window.matchMedia("(prefers-reduced-motion: reduce)").matches
    });

    const menuToggle = document.querySelector(".menu-toggle");
    const navigation = document.querySelector(".navigation");
    const menuIcon = menuToggle.querySelector("i");
    const navigationLinks = navigation.querySelectorAll("a");

    const closeMenu = () => {
        navigation.classList.remove("open");
        menuToggle.setAttribute("aria-expanded", "false");
        menuToggle.setAttribute("aria-label", "Open navigation");
        menuIcon.classList.replace("bx-x", "bx-menu");
    };

    menuToggle.addEventListener("click", () => {
        const isOpen = navigation.classList.toggle("open");

        menuToggle.setAttribute("aria-expanded", String(isOpen));
        menuToggle.setAttribute(
            "aria-label",
            isOpen ? "Close navigation" : "Open navigation"
        );

        menuIcon.classList.toggle("bx-menu", !isOpen);
        menuIcon.classList.toggle("bx-x", isOpen);
    });

    navigationLinks.forEach(link => {
        link.addEventListener("click", closeMenu);
    });

    document.addEventListener("keydown", event => {
        if (event.key === "Escape") {
            closeMenu();
        }
    });

    window.addEventListener("resize", () => {
        if (window.innerWidth > 760) {
            closeMenu();
        }
    });
});