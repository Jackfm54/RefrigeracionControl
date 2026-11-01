
document.addEventListener("DOMContentLoaded", () => {

    const sidebar = document.getElementById("sidebar");
    const overlay = document.getElementById("sidebarOverlay");
    const menuToggle = document.getElementById("menuToggle");
    const themeToggle = document.getElementById("themeToggle");

    const root = document.documentElement;

    // Recuperar tema guardado
    const storedTheme = localStorage.getItem("rc-theme");

    if (storedTheme === "light" || storedTheme === "dark") {
        root.setAttribute("data-theme", storedTheme);
    }

    // Cambiar tema
    themeToggle?.addEventListener("click", () => {
        const currentTheme = root.getAttribute("data-theme");
        const nextTheme = currentTheme === "dark" ? "light" : "dark";

        root.setAttribute("data-theme", nextTheme);
        localStorage.setItem("rc-theme", nextTheme);
    });

    function closeSidebar() {
        sidebar?.classList.remove("open");
        overlay?.classList.remove("open");
        menuToggle?.setAttribute("aria-expanded", "false");
    }

    // Abrir y cerrar el menú móvil
    menuToggle?.addEventListener("click", () => {
        const isOpen = sidebar?.classList.toggle("open");

        overlay?.classList.toggle("open", isOpen);
        menuToggle.setAttribute(
            "aria-expanded",
            String(Boolean(isOpen))
        );
    });

    overlay?.addEventListener("click", closeSidebar);

    document.addEventListener("keydown", (event) => {
        if (event.key === "Escape") {
            closeSidebar();
        }
    });

    // Resaltar menú correspondiente a la página actual
    const currentPath = window.location.pathname
        .toLowerCase()
        .replace(/\/+$/, "");

    document.querySelectorAll(
        ".sidebar-nav a.nav-item-custom"
    ).forEach(link => {
        const linkPath = new URL(link.href)
            .pathname
            .toLowerCase()
            .replace(/\/+$/, "");

        if (currentPath === linkPath ||
            (currentPath === "/dashboard/index" &&
                linkPath === "/dashboard")) {
            link.classList.add("active");
        }
    });
});
