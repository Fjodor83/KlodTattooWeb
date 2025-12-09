// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener("DOMContentLoaded", function () {
    const navbar = document.querySelector(".navbar[data-dynamic-navbar='true']");

    if (navbar) {
        window.addEventListener("scroll", function () {
            if (window.scrollY > 50) {
                // Scrolled down: Glass effect
                navbar.classList.remove("navbar-transparent");
                navbar.classList.add("navbar-glass");
                navbar.classList.remove("shadow-none");
                navbar.classList.add("shadow-sm");
            } else {
                // At top: Transparent
                navbar.classList.add("navbar-transparent");
                navbar.classList.remove("navbar-glass");
                navbar.classList.add("shadow-none");
                navbar.classList.remove("shadow-sm");
            }
        });
    }
});
