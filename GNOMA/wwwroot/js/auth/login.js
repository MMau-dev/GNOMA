
document.addEventListener("DOMContentLoaded", () => {
    const passwordInput = document.getElementById("loginPassword");
    const togglePassword = document.getElementById("togglePassword");

    if (!passwordInput || !togglePassword) {
        return;
    }

    const icon = togglePassword.querySelector("i");

    togglePassword.addEventListener("click", () => {
        const showPassword = passwordInput.type === "password";

        passwordInput.type = showPassword ? "text" : "password";

        icon.classList.toggle("bx-show", !showPassword);
        icon.classList.toggle("bx-hide", showPassword);

        togglePassword.setAttribute(
            "aria-label",
            showPassword ? "Hide password" : "Show password"
        );

        togglePassword.setAttribute(
            "aria-pressed",
            String(showPassword)
        );
    });
});

document.addEventListener("DOMContentLoaded", () => {
    const toggleButtons = document.querySelectorAll("[data-password-toggle]");

    toggleButtons.forEach((button) => {
        button.addEventListener("click", () => {
            const inputId = button.dataset.passwordToggle;
            const passwordInput = document.getElementById(inputId);

            if (!passwordInput) {
                return;
            }

            const showPassword = passwordInput.type === "password";

            passwordInput.type = showPassword ? "text" : "password";
            button.setAttribute("aria-pressed", String(showPassword));
            button.setAttribute(
                "aria-label",
                showPassword ? "Hide password" : "Show password"
            );

            const icon = button.querySelector("i");

            if (icon) {
                icon.classList.toggle("bx-show", !showPassword);
                icon.classList.toggle("bx-hide", showPassword);
            }
        });
    });
});
