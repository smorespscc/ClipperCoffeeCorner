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

  const username = document.getElementById('username').value.trim();
  const email = document.getElementById('email').value.trim();
  const staffCode = staffCodeInput.value.trim();

  const emailValid = /^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(email);

  let isValid = true;

  // Username
  if (!username) {
    document.getElementById('username').classList.add('is-invalid');
    isValid = false;
  } else {
    document.getElementById('username').classList.remove('is-invalid');
  }

  // Email
  if (!emailValid) {
    document.getElementById('email').classList.add('is-invalid');
    isValid = false;
  } else {
    document.getElementById('email').classList.remove('is-invalid');
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

