(function () {

    "use strict";


    function initializeThemeToggle() {

        const button =
            document.getElementById("piqThemeToggle");

        const icon =
            document.getElementById("piqThemeIcon");


        if (!button || !icon) {
            return;
        }


        const html =
            document.documentElement;


        function updateIcon(theme) {

            if (theme === "dark") {

                icon.className = "bi bi-sun";

                button.setAttribute(
                    "aria-label",
                    "Switch to light mode"
                );

                button.setAttribute(
                    "title",
                    "Switch to light mode"
                );

            }
            else {

                icon.className = "bi bi-moon";

                button.setAttribute(
                    "aria-label",
                    "Switch to dark mode"
                );

                button.setAttribute(
                    "title",
                    "Switch to dark mode"
                );

            }

        }


        function getCurrentTheme() {

            return html.getAttribute("data-bs-theme")
                || "light";

        }


        updateIcon(getCurrentTheme());


        button.addEventListener(
            "click",
            function () {

                const currentTheme =
                    getCurrentTheme();

                const newTheme =
                    currentTheme === "dark"
                        ? "light"
                        : "dark";


                html.setAttribute(
                    "data-bs-theme",
                    newTheme
                );


                localStorage.setItem(
                    "piq-theme",
                    newTheme
                );


                updateIcon(newTheme);

            }
        );

    }


    if (document.readyState === "loading") {

        document.addEventListener(
            "DOMContentLoaded",
            initializeThemeToggle
        );

    }
    else {

        initializeThemeToggle();

    }

})();