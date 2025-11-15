function formatCurrency(n) {
    return '$' + Number(n || 0).toFixed(2);
}

const basePrices = {
    'Cappuccino': 4.50,
    'Vanilla Latte': 4.95,
    'House Brew': 3.50,
    'Chai Tea Latte': 4.75,
    'Iced Mocha': 5.25,
    'Double Espresso': 3.25,
    'Breakfast Sandwich': 5.50,
    'Bagel & Cream Cheese': 3.25,
    'Pancake Stack': 6.50,
    'Cheese Omelette': 6.00,
    'Butter Croissant': 2.75,
    'Blueberry Muffin': 2.95
};

function modifierAdd(mods) {
    if (!Array.isArray(mods)) return 0;
    return mods.reduce((sum, m) => {
        if (/extra shot/i.test(m)) return sum + 0.75;
        if (/oat/i.test(m)) return sum + 0.50;
        if (/whip/i.test(m)) return sum + 0.25;
        return sum;
    }, 0);
}

function populateOrderSummary() {
    const tbody = document.getElementById('orderSummary');
    const totalCell = document.getElementById('orderTotal');
    const items = JSON.parse(localStorage.getItem('selectedItems') || '[]');
    const isStaff = localStorage.getItem('isStaff') === 'true';

    if (!Array.isArray(items) || items.length === 0) {
        tbody.innerHTML = `
      <tr>
        <td colspan="5" class="text-center text-muted">No items found. Please return to the menu.</td>
      </tr>
    `;
        totalCell.textContent = formatCurrency(0);
        return;
    }

    let subtotal = 0;
    const rows = items.map(it => {
        const modsText = (Array.isArray(it.modifiers) && it.modifiers.length) ? it.modifiers.join(', ') : '—';
        const specialSafe = it.specialRequests && it.specialRequests.trim() ? it.specialRequests.trim() : '—';
        const base = basePrices[it.name] ?? 4.00;
        const unitPrice = base + modifierAdd(it.modifiers);
        const quantity = it.quantity || 1;
        const totalPrice = unitPrice * quantity;
        subtotal += totalPrice;
        return `
      <tr>
        <td>${it.name}</td>
        <td class="text-center">${quantity}</td>
        <td>${modsText}</td>
        <td>${specialSafe}</td>
        <td class="text-end">${formatCurrency(totalPrice)}</td>
      </tr>
    `;
    });

    tbody.innerHTML = rows.join('');

    let discount = 0;
    if (isStaff) {
        discount = subtotal * 0.10;
    }
    const finalTotal = subtotal - discount;

    const tfoot = tbody.parentElement.querySelector('tfoot');
    tfoot.innerHTML = `
    <tr>
      <th colspan="4" class="text-end">Subtotal</th>
      <th class="text-end">${formatCurrency(subtotal)}</th>
    </tr>
    ${isStaff ? `
    <tr>
      <th colspan="4" class="text-end text-success">Staff Discount (10%)</th>
      <th class="text-end text-success">- ${formatCurrency(discount)}</th>
    </tr>` : ''}
    <tr>
      <th colspan="4" class="text-end">Total</th>
      <th id="orderTotal" class="text-end">${formatCurrency(finalTotal)}</th>
    </tr>
  `;
}

document.addEventListener('DOMContentLoaded', populateOrderSummary);

const paymentForm = document.getElementById('paymentForm');
paymentForm.addEventListener('submit', function (e) {
    e.preventDefault();

    const cardNumber = document.getElementById('cardNumber').value.trim();
    const expiry = document.getElementById('expiry').value.trim();
    const cvv = document.getElementById('cvv').value.trim();

    const cardValid = /^\d{16}$/.test(cardNumber.replace(/\s+/g, ''));
    const expiryValid = /^(0[1-9]|1[0-2])\/\d{2}$/.test(expiry);
    const cvvValid = /^\d{3}$/.test(cvv);

    if (!cardValid || !expiryValid || !cvvValid) {
        const errorModal = new bootstrap.Modal(document.getElementById('paymentErrorModal'));
        errorModal.show();
        return;
    }

    const isSuccess = Math.random() > 0.5;

    if (isSuccess) {
        const successModal = new bootstrap.Modal(document.getElementById('paymentSuccessModal'));
        successModal.show();

        const paidItems = JSON.parse(localStorage.getItem('selectedItems') || '[]');
        const paidSpecial = localStorage.getItem('specialRequests') || '';
        const orderNumber = Math.floor(Math.random() * 900 + 100);

        localStorage.setItem('lastOrder', JSON.stringify({
            orderNumber,
            items: paidItems,
            specialRequests: paidSpecial,
            paidAt: Date.now()
        }));

        localStorage.removeItem('selectedItems');
        localStorage.removeItem('specialRequests');

        setTimeout(() => {
            window.location.href = 'queue.html';
        }, 2000);
    } else {
        const errorModal = new bootstrap.Modal(document.getElementById('paymentErrorModal'));
        errorModal.show();
    }
});
