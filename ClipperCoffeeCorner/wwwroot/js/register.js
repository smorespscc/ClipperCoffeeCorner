const registerForm = document.getElementById('registerForm');

// Password validation requirements
const passwordRequirements = {
    length: { regex: /.{8,}/, element: 'req-length' },
    uppercase: { regex: /[A-Z]/, element: 'req-uppercase' },
    lowercase: { regex: /[a-z]/, element: 'req-lowercase' },
    number: { regex: /\d/, element: 'req-number' },
    special: { regex: /[!@#$%^&*(),.?":{}|<>]/, element: 'req-special' }
};

// Load saved notification preferences on page load
document.addEventListener('DOMContentLoaded', () => {
    const emailConsent = localStorage.getItem('emailNotificationsConsent') === 'true';
    const textConsent = localStorage.getItem('textNotificationsConsent') === 'true';
    
    document.getElementById('emailNotifications').checked = emailConsent;
    document.getElementById('textNotifications').checked = textConsent;

    // Set up password validation
    const passwordInput = document.getElementById('password');
    const confirmPasswordInput = document.getElementById('confirmPassword');

    passwordInput.addEventListener('input', validatePassword);
    confirmPasswordInput.addEventListener('input', validatePasswordMatch);
});

function validatePassword() {
    const password = document.getElementById('password').value;
    const strengthFill = document.getElementById('strengthFill');
    const strengthText = document.getElementById('strengthText');
    
    let metRequirements = 0;
    let totalRequirements = Object.keys(passwordRequirements).length;

    // Check each requirement
    Object.entries(passwordRequirements).forEach(([key, requirement]) => {
        const element = document.getElementById(requirement.element);
        const isMet = requirement.regex.test(password);
        
        if (isMet) {
            element.classList.add('met');
            metRequirements++;
        } else {
            element.classList.remove('met');
        }
    });

    // Update strength indicator
    const strengthPercentage = (metRequirements / totalRequirements) * 100;
    
    // Remove all strength classes
    strengthFill.className = 'strength-fill';
    strengthText.className = 'strength-text';
    
    if (strengthPercentage === 0) {
        strengthText.textContent = 'Password strength';
    } else if (strengthPercentage < 40) {
        strengthFill.classList.add('weak');
        strengthText.classList.add('weak');
        strengthText.textContent = 'Weak password';
    } else if (strengthPercentage < 60) {
        strengthFill.classList.add('fair');
        strengthText.classList.add('fair');
        strengthText.textContent = 'Fair password';
    } else if (strengthPercentage < 100) {
        strengthFill.classList.add('good');
        strengthText.classList.add('good');
        strengthText.textContent = 'Good password';
    } else {
        strengthFill.classList.add('strong');
        strengthText.classList.add('strong');
        strengthText.textContent = 'Strong password';
    }

    return metRequirements === totalRequirements;
}

function validatePasswordMatch() {
    const password = document.getElementById('password').value;
    const confirmPassword = document.getElementById('confirmPassword').value;
    const confirmPasswordInput = document.getElementById('confirmPassword');

    if (confirmPassword && password !== confirmPassword) {
        confirmPasswordInput.classList.add('is-invalid');
        return false;
    } else if (confirmPassword) {
        confirmPasswordInput.classList.remove('is-invalid');
        confirmPasswordInput.classList.add('is-valid');
        return true;
    }
    
    confirmPasswordInput.classList.remove('is-invalid', 'is-valid');
    return false;
}

registerForm.addEventListener('submit', function (e) {
    e.preventDefault();

    // Grab values
    const username = document.getElementById('username').value.trim();
    const phone = document.getElementById('phone').value.trim();
    const email = document.getElementById('email').value.trim();
    const password = document.getElementById('password').value;
    const confirmPassword = document.getElementById('confirmPassword').value;

    // Validation checks
    const phoneValid = /^\d{3}-?\d{3}-?\d{4}$/.test(phone);
    const emailValid = /^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(email);
    const passwordValid = validatePassword();
    const passwordsMatch = validatePasswordMatch();

    let isValid = true;

    // Username validation
    if (!username) {
        document.getElementById('username').classList.add('is-invalid');
        isValid = false;
    } else {
        document.getElementById('username').classList.remove('is-invalid');
        document.getElementById('username').classList.add('is-valid');
    }

    // Phone validation
    if (!phoneValid) {
        document.getElementById('phone').classList.add('is-invalid');
        isValid = false;
    } else {
        document.getElementById('phone').classList.remove('is-invalid');
        document.getElementById('phone').classList.add('is-valid');
    }

    // Email validation
    if (!emailValid) {
        document.getElementById('email').classList.add('is-invalid');
        isValid = false;
    } else {
        document.getElementById('email').classList.remove('is-invalid');
        document.getElementById('email').classList.add('is-valid');
    }

    // Password validation
    if (!passwordValid) {
        document.getElementById('password').classList.add('is-invalid');
        isValid = false;
    } else {
        document.getElementById('password').classList.remove('is-invalid');
        document.getElementById('password').classList.add('is-valid');
    }

    // Password match validation
    if (!passwordsMatch || !confirmPassword) {
        document.getElementById('confirmPassword').classList.add('is-invalid');
        isValid = false;
    }

    if (isValid) {
        // Save user data to localStorage
        const userData = {
            username: username,
            phone: phone,
            email: email,
            password: password, // In production, this should be hashed
            registrationDate: new Date().toISOString()
        };

        // Save notification preferences
        const emailNotifications = document.getElementById('emailNotifications').checked;
        const textNotifications = document.getElementById('textNotifications').checked;
        
        localStorage.setItem('userData', JSON.stringify(userData));
        localStorage.setItem('emailNotificationsConsent', emailNotifications.toString());
        localStorage.setItem('textNotificationsConsent', textNotifications.toString());

        // Show success modal
        const successModal = new bootstrap.Modal(document.getElementById('registerSuccessModal'));
        successModal.show();

        // Redirect to login page after 2 seconds
        setTimeout(() => {
            window.location.href = '/Home/Index';
        }, 2000);
    }
});
