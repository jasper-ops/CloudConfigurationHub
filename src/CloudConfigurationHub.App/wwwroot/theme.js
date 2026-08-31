const storageKey = "cloud-configuration-hub.theme";
const supportedThemes = new Set(["light", "dark", "system"]);
const mediaQuery = window.matchMedia("(prefers-color-scheme: dark)");

function normalizeTheme(theme) {
    return supportedThemes.has(theme) ? theme : "system";
}

function resolveTheme(theme) {
    return theme === "system"
        ? mediaQuery.matches ? "dark" : "light"
        : theme;
}

export function readTheme() {
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
    document.querySelectorAll("[data-theme-option]").forEach(button => {
        const active = button.getAttribute("data-theme-option") === theme;
        button.classList.toggle("active", active);
        button.setAttribute("aria-pressed", active ? "true" : "false");
    });
}

export function applyTheme(theme) {
    const normalizedTheme = normalizeTheme(theme);
    const resolvedTheme = resolveTheme(normalizedTheme);
    const root = document.documentElement;

    root.setAttribute("data-theme-mode", normalizedTheme);
    root.setAttribute("data-theme", resolvedTheme);
    root.style.colorScheme = resolvedTheme;
    markActive(normalizedTheme);
}

export function setTheme(theme) {
    const normalizedTheme = normalizeTheme(theme);
    persistTheme(normalizedTheme);
    applyTheme(normalizedTheme);
}

function restoreTheme() {
    applyTheme(readTheme());
}

document.addEventListener("click", event => {
    if (!(event.target instanceof Element)) {
        return;
    }

    const button = event.target.closest("button[data-theme-option]");
    if (!(button instanceof HTMLButtonElement)) {
        return;
    }

    setTheme(button.dataset.themeOption);
});

mediaQuery.addEventListener("change", () => {
    if (readTheme() === "system") {
        applyTheme("system");
    }
});

document.addEventListener("enhancedload", restoreTheme);
window.addEventListener("pageshow", restoreTheme);

if (window.Blazor && typeof window.Blazor.addEventListener === "function") {
    window.Blazor.addEventListener("enhancedload", restoreTheme);
}

restoreTheme();
