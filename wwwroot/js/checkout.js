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

function rowHTML(item, index) {
    const modsText = (Array.isArray(item.modifiers) && item.modifiers.length) ? item.modifiers.join(', ') : '—';
    const specialSafe = item.specialRequests && item.specialRequests.trim() ? item.specialRequests.trim() : '—';
    const base = basePrices[item.name] ?? 4.00;
    const unitPrice = base + modifierAdd(item.modifiers);
    const quantity = item.quantity || 1;
    const totalPrice = unitPrice * quantity;
    return `
    <tr>
      <td>${item.name}</td>
      <td class="text-center">${quantity}</td>
      <td>${modsText}</td>
      <td>${specialSafe}</td>
      <td class="text-end">${formatCurrency(totalPrice)}</td>
      <td><button class="btn btn-sm btn-danger remove-btn" data-index="${index}">Remove</button></td>
    </tr>
  `;
}

function populateFromLocalStorage() {
    const tbody = document.getElementById('orderSummary');
    const footer = document.getElementById('orderFooter');
    const totalCell = document.getElementById('estimatedTotal');
    const selectedItems = JSON.parse(localStorage.getItem('selectedItems') || '[]');

    // If no items, clear and redirect immediately
    if (!Array.isArray(selectedItems) || selectedItems.length === 0) {
        localStorage.removeItem('lastOrder');
        localStorage.removeItem('selectedItems');
        localStorage.removeItem('specialRequests');

        // Redirect to menu and stop further execution
        window.location.href = '/Home/Menu';
        return;
    }

    const globalSpecial = localStorage.getItem('specialRequests') || '';
    const isStaff = localStorage.getItem('isStaff') === 'true';
    const globalSpecialContainer = document.getElementById('globalSpecialContainer');
    const globalSpecialText = document.getElementById('globalSpecialText');

    if (globalSpecial && globalSpecial.trim()) {
        globalSpecialText.textContent = globalSpecial.trim();
        globalSpecialContainer.classList.remove('d-none');
    } else {
        globalSpecialContainer.classList.add('d-none');
    }

    // Generate table rows
    let total = 0;
    const rows = selectedItems.map((item, index) => {
        const base = basePrices[item.name] ?? 4.00;
        const unitPrice = base + modifierAdd(item.modifiers);
        const quantity = item.quantity || 1;
        const itemTotal = unitPrice * quantity;
        total += itemTotal;
        return rowHTML(item, index);
    });

    tbody.innerHTML = rows.join('');

    // Apply staff discount if applicable
    let discount = 0;
    if (isStaff) {
        discount = total * 0.10;
    }
    const finalTotal = total - discount;

    // Build footer rows
    footer.innerHTML = `
    <tr>
      <th colspan="5" class="text-end">Subtotal</th>
      <th class="text-end">${formatCurrency(total)}</th>
    </tr>
    ${isStaff ? `
    <tr>
      <th colspan="5" class="text-end text-success">Staff Discount (10%)</th>
      <th class="text-end text-success">- ${formatCurrency(discount)}</th>
    </tr>` : ''}
    <tr>
      <th colspan="5" class="text-end">Estimated Total</th>
      <th id="estimatedTotal" class="text-end">${formatCurrency(finalTotal)}</th>
    </tr>
  `;

    // Attach remove handlers
    document.querySelectorAll('.remove-btn').forEach(btn => {
        btn.addEventListener('click', function () {
            const index = parseInt(this.getAttribute('data-index'));
            const items = JSON.parse(localStorage.getItem('selectedItems') || '[]');
            items.splice(index, 1);
            localStorage.setItem('selectedItems', JSON.stringify(items));
            populateFromLocalStorage();
        });
    });
}


document.addEventListener('DOMContentLoaded', populateFromLocalStorage);

// Go back to menu without clearing cart
document.getElementById('backBtn').addEventListener('click', () => {
    window.location.href = '/Home/Menu?from=checkout';
});

// Clear all button
document.getElementById('clearAllBtn').addEventListener('click', () => {
    localStorage.removeItem('selectedItems');
    localStorage.removeItem('specialRequests');
    populateFromLocalStorage();
});

// Proceed to payment button
document.getElementById('proceedToPaymentBtn').addEventListener('click', () => {
    window.location.href = '/Home/Payment';
});
