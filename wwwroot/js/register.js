const registerForm = document.getElementById('registerForm');

registerForm.addEventListener('submit', function (e) {
    e.preventDefault();

    // Grab values
    const username = document.getElementById('username').value.trim();
    const phone = document.getElementById('phone').value.trim();
    const email = document.getElementById('email').value.trim();

    // Regex checks
    const phoneValid = /^\d{3}-?\d{3}-?\d{4}$/.test(phone); // allows 1234567890 or 123-456-7890
    const emailValid = /^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(email);

    let isValid = true;

    if (!username) {
        document.getElementById('username').classList.add('is-invalid');
        isValid = false;
    } else {
        document.getElementById('username').classList.remove('is-invalid');
    }

    if (!phoneValid) {
        document.getElementById('phone').classList.add('is-invalid');
        isValid = false;
    } else {
        document.getElementById('phone').classList.remove('is-invalid');
    }

    if (!emailValid) {
        document.getElementById('email').classList.add('is-invalid');
        isValid = false;
    } else {
        document.getElementById('email').classList.remove('is-invalid');
    }

    if (isValid) {
        // Show success modal
        const successModal = new bootstrap.Modal(document.getElementById('registerSuccessModal'));
        successModal.show();

        // Redirect to login page after 2 seconds
        setTimeout(() => {
            window.location.href = '../index.html';
        }, 2000);
    }
});
