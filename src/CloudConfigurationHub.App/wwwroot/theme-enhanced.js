(function () {
    const storageKey = "cloud-configuration-hub.theme";
    const mediaQuery = window.matchMedia("(prefers-color-scheme: dark)");

    function normalizeTheme(theme) {
        return theme === "light" || theme === "dark" || theme === "system"
            ? theme
            : "system";
    }

    function resolveTheme(theme) {
        if (theme === "dark") {
            return "dark";
        }

        if (theme === "light") {
            return "light";
        }

        return mediaQuery.matches ? "dark" : "light";
    }

    function readTheme() {
        try {
            return normalizeTheme(localStorage.getItem(storageKey));
        }
        catch {
            return "system";
        }
    }

    function applyTheme(theme) {
        const normalized = normalizeTheme(theme);
        const resolved = resolveTheme(normalized);
        document.documentElement.setAttribute("data-theme-mode", normalized);
        document.documentElement.setAttribute("data-theme", resolved);
        document.documentElement.style.colorScheme = resolved;
        document.querySelectorAll("[data-theme-option]").forEach(function (button) {
            const active = button.getAttribute("data-theme-option") === normalized;
            button.classList.toggle("active", active);
            button.setAttribute("aria-pressed", active ? "true" : "false");
        });
    }

    function restoreTheme() {
        applyTheme(readTheme());
    }

    if (window.Blazor && typeof window.Blazor.addEventListener === "function") {
        window.Blazor.addEventListener("enhancedload", restoreTheme);
    }

    document.addEventListener("enhancedload", restoreTheme);
    window.addEventListener("pageshow", restoreTheme);
    restoreTheme();
})();
