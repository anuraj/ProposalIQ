(function () {

    const storageKey = "piq-theme";

    const savedTheme = localStorage.getItem(storageKey);

    if (savedTheme === "dark" || savedTheme === "light") {

        document.documentElement.setAttribute(
            "data-bs-theme",
            savedTheme
        );

        return;
    }


    const prefersDark =
        window.matchMedia &&
        window.matchMedia("(prefers-color-scheme: dark)").matches;


    document.documentElement.setAttribute(
        "data-bs-theme",
        prefersDark ? "dark" : "light"
    );

})();