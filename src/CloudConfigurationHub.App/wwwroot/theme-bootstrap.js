{
    const storageKey = "cloud-configuration-hub.theme";
    const supportedThemes = new Set(["light", "dark", "system"]);
    let theme = "system";

    try {
        const storedTheme = localStorage.getItem(storageKey);
        theme = supportedThemes.has(storedTheme) ? storedTheme : "system";
    }
    catch {
        // 浏览器禁用存储时使用系统主题。
    }

    const resolvedTheme = theme === "system"
        ? window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light"
        : theme;

    document.documentElement.setAttribute("data-theme-mode", theme);
    document.documentElement.setAttribute("data-theme", resolvedTheme);
    document.documentElement.style.colorScheme = resolvedTheme;
}
