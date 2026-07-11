(function () {
    const storageKey = "cloud-configuration-hub.theme";
    const root = document.documentElement;
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

    function persistTheme(theme) {
        try {
            localStorage.setItem(storageKey, theme);
        }
        catch {
            // 浏览器禁用存储时仍允许本次页面会话应用主题。
        }
    }

    function markActive(theme) {
        document.querySelectorAll("[data-theme-option]").forEach(function (button) {
            const active = button.getAttribute("data-theme-option") === theme;
            button.classList.toggle("active", active);
            button.setAttribute("aria-pressed", active ? "true" : "false");
        });
    }

    function applyTheme(theme) {
        const normalized = normalizeTheme(theme);
        const resolved = resolveTheme(normalized);
        root.setAttribute("data-theme-mode", normalized);
        root.setAttribute("data-theme", resolved);
        root.style.colorScheme = resolved;
        markActive(normalized);
    }

    function setTheme(theme) {
        const normalized = normalizeTheme(theme);
        persistTheme(normalized);
        applyTheme(normalized);
    }

    function bindSwitcher() {
        document.querySelectorAll("[data-theme-option]").forEach(function (button) {
            if (button.dataset.themeBound === "true") {
                return;
            }

            button.dataset.themeBound = "true";
            button.addEventListener("click", function () {
                setTheme(button.getAttribute("data-theme-option"));
            });
        });
        markActive(readTheme());
    }

    applyTheme(readTheme());
    mediaQuery.addEventListener("change", function () {
        if (readTheme() === "system") {
            applyTheme("system");
        }
    });

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", bindSwitcher);
    }
    else {
        bindSwitcher();
    }

    document.addEventListener("enhancedload", function () {
        applyTheme(readTheme());
        bindSwitcher();
    });
    window.addEventListener("pageshow", function () {
        applyTheme(readTheme());
    });

    window.CloudConfigurationHubTheme = {
        applyTheme,
        setTheme,
        readTheme
    };
})();
