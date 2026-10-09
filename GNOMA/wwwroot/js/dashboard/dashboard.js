
document.addEventListener("DOMContentLoaded", () => {
    const sidebar = document.getElementById("dashboardSidebar");
    const toggle = document.getElementById("sidebarToggle");
    const overlay = document.getElementById("sidebarOverlay");
    const navigationLinks = sidebar.querySelectorAll(".sidebar-link");

    const isMobile = () => window.matchMedia("(max-width: 760px)").matches;

    const openSidebar = () => {
        if (!isMobile()) return;

        sidebar.classList.add("is-open");
        overlay.classList.add("is-visible");
        toggle.setAttribute("aria-expanded", "true");
        toggle.setAttribute("aria-label", "Close navigation");
        document.body.style.overflow = "hidden";
    };

    const closeSidebar = () => {
        sidebar.classList.remove("is-open");
        overlay.classList.remove("is-visible");
        toggle.setAttribute("aria-expanded", "false");
        toggle.setAttribute("aria-label", "Open navigation");
        document.body.style.overflow = "";
    };

    toggle.addEventListener("click", () => {
        if (sidebar.classList.contains("is-open")) {
            closeSidebar();
        } else {
            openSidebar();
        }
    });

    overlay.addEventListener("click", closeSidebar);

    navigationLinks.forEach(link => {
        link.addEventListener("click", () => {
            if (isMobile()) {
                closeSidebar();
            }
        });
    });

    document.addEventListener("keydown", event => {
        if (event.key === "Escape") {
            closeSidebar();
        }
    });

    window.addEventListener("resize", () => {
        if (!isMobile()) {
            closeSidebar();
        }
    });
});