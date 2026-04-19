// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

document.addEventListener("DOMContentLoaded", () => {
    const sidebarToggle = document.querySelector("[data-sidebar-toggle]");
    const sidebar = document.getElementById("appSidebar");

    if (sidebarToggle && sidebar) {
        sidebarToggle.addEventListener("click", () => sidebar.classList.toggle("is-open"));
    }

    document.querySelectorAll("[data-confirm]").forEach((element) => {
        element.addEventListener("click", (event) => {
            const message = element.getAttribute("data-confirm") || "Are you sure?";
            if (!window.confirm(message)) {
                event.preventDefault();
            }
        });
    });
});
