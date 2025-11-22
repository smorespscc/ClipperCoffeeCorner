const loginForm = document.getElementById('loginForm');
const isStaffCheckbox = document.getElementById('isStaff');
const staffCodeWrapper = document.getElementById('staffCodeWrapper');
const staffCodeInput = document.getElementById('staffCode');

// Example valid staff codes (in real life, check against DB)
const validStaffCodes = ["STAFF123", "BARISTA2025", "MANAGER01"];

// Toggle staff code field visibility
isStaffCheckbox.addEventListener('change', () => {
  if (isStaffCheckbox.checked) {
    staffCodeWrapper.classList.remove('d-none');
  } else {
    staffCodeWrapper.classList.add('d-none');
    staffCodeInput.classList.remove('is-invalid');
  }
});

loginForm.addEventListener('submit', function (e) {
  e.preventDefault();

  const email = document.getElementById('email').value.trim();
  const password = document.getElementById('password').value.trim();
  const staffCode = staffCodeInput.value.trim();

  let isValid = true;

  // Email validation
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  if (!email) {
    document.getElementById('email').classList.add('is-invalid');
    document.getElementById('email').nextElementSibling.textContent = 'Please enter a valid email address.';
    isValid = false;
  } else if (!emailRegex.test(email)) {
    document.getElementById('email').classList.add('is-invalid');
    document.getElementById('email').nextElementSibling.textContent = 'Please enter a valid email address.';
    isValid = false;
  } else {
    document.getElementById('email').classList.remove('is-invalid');
  }

  // Password validation - check against stored user data
  const userData = JSON.parse(localStorage.getItem('userData') || '{}');

  if (!password) {
    document.getElementById('password').classList.add('is-invalid');
    isValid = false;
  } else {
    // Check if user exists and password matches
    if (userData.email) {
      // Check if email matches and password is correct
      const emailMatches = email === userData.email;
      
      if (emailMatches && password === userData.password) {
        document.getElementById('password').classList.remove('is-invalid');
      } else if (password === 'password') {
        // Fallback for demo purposes
        document.getElementById('password').classList.remove('is-invalid');
      } else {
        document.getElementById('password').classList.add('is-invalid');
        isValid = false;
      }
    } else if (password === 'password') {
      // Fallback for demo purposes when no user is registered
      document.getElementById('password').classList.remove('is-invalid');
    } else {
      document.getElementById('password').classList.add('is-invalid');
      isValid = false;
    }
  }

  // Staff code validation if staff is checked
  if (isStaffCheckbox.checked) {
    if (!staffCode) {
      staffCodeInput.classList.add('is-invalid');
      // Update the invalid-feedback message with a dummy clue
      staffCodeInput.nextElementSibling.textContent =
        "Staff code is required and must be valid. Please verify with the café front desk if unsure.\n(For testing, try 9999)";
      isValid = false;
    } else {
      staffCodeInput.classList.remove('is-invalid');
    }
  }

  if (isValid) {
    // Check for debug email
    if (email === 'spscc@edu.com') {
      localStorage.setItem('debugMode', 'true');
    } else {
      localStorage.removeItem('debugMode');
    }

    // Save staff flag and code for later use
    if (isStaffCheckbox.checked) {
      localStorage.setItem('isStaff', 'true');
      localStorage.setItem('staffCode', staffCode);
    } else {
      localStorage.removeItem('isStaff');
      localStorage.removeItem('staffCode');
    }

    // Show success modal
    const successModal = new bootstrap.Modal(document.getElementById('loginSuccessModal'));
    successModal.show();

    // Redirect to menu page after 2 seconds
    setTimeout(() => {
      window.location.href = '/Home/Menu';
    }, 2000);
  }
});

// Idle redirect to /pages/queue.html after 60 seconds of no activity
let idleTimer;
const IDLE_LIMIT = 60000; // 1 minute (adjust as needed)

function resetIdleTimer() {
  clearTimeout(idleTimer);
  idleTimer = setTimeout(() => {
    // Flag that this is a "resting" redirect
    localStorage.setItem('restingMode', 'true');
    window.location.href = '/Home/Queue';
  }, IDLE_LIMIT);
}

// Reset timer on any interaction
['mousemove', 'keydown', 'click', 'scroll', 'touchstart'].forEach(evt => {
  document.addEventListener(evt, resetIdleTimer, { passive: true });
});

// Start timer on load
resetIdleTimer();

