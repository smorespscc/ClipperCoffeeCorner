function showReadyModal() {
    const modal = new bootstrap.Modal(document.getElementById('readyModal'));
    modal.show();
}

function buildPlaceholderQueue(baseNum = 200) {
    return [
        { num: baseNum - 3, drink: 'Cappuccino', status: 'Preparing', badge: 'bg-warning text-dark' },
        { num: baseNum - 2, drink: 'Latte', status: 'Ready', badge: 'bg-success' },
        { num: baseNum - 1, drink: 'Matcha Tea', status: 'Queued', badge: 'bg-secondary' },
        { num: baseNum, drink: 'Americano', status: 'Preparing', badge: 'bg-warning text-dark' },
        { num: baseNum + 1, drink: 'Espresso', status: 'Queued', badge: 'bg-secondary' },
        { num: baseNum + 2, drink: 'Mocha', status: 'Preparing', badge: 'bg-warning text-dark' },
        { num: baseNum + 3, drink: 'Flat White', status: 'Queued', badge: 'bg-secondary' }
    ];
}

function populateQueue() {
    const userSummary = document.getElementById('userSummary');
    const footer = document.getElementById('queueFooter');
    const orderNumberEl = document.getElementById('orderNumber');
    const queuePositionEl = document.getElementById('queuePosition');
    const waitTimeEl = document.getElementById('waitTime');
    const tbody = document.getElementById('queueTableBody');

    const lastOrder = JSON.parse(localStorage.getItem('lastOrder') || 'null');
    const selectedItems = JSON.parse(localStorage.getItem('selectedItems') || '[]');

    // Always show placeholders
    const placeholders = buildPlaceholderQueue();

    let items = [];
    let orderNumber = null;
    if (lastOrder && Array.isArray(lastOrder.items) && lastOrder.items.length > 0) {
        items = lastOrder.items;
        orderNumber = lastOrder.orderNumber || Math.floor(Math.random() * 900 + 100);
    } else if (Array.isArray(selectedItems) && selectedItems.length > 0) {
        items = selectedItems;
        orderNumber = Math.floor(Math.random() * 900 + 100);
    }

    // Show user summary/footer if user has an order
    if (items.length > 0) {
        userSummary.classList.remove('d-none');
        footer.classList.remove('d-none');

        orderNumberEl.textContent = '#' + orderNumber;
        queuePositionEl.textContent = '3rd';
        waitTimeEl.textContent = '5';

        const userDrink = items.length > 1 ? 'Multiple Items' : (items[0]?.name || 'Item');

        // Insert user order in the middle of placeholder queue
        placeholders.splice(3, 0, { num: orderNumber, drink: userDrink, status: 'Queued', badge: 'bg-secondary', highlight: true });
    } else {
        userSummary.classList.add('d-none');
        footer.classList.add('d-none');
    }

    // Populate table
    tbody.innerHTML = placeholders.map(q => `
        <tr ${q.highlight ? 'class="table-info"' : ''}>
            <td class="text-center">#${q.num}</td>
            <td class="text-center">${q.drink}</td>
            <td class="text-center"><span class="badge ${q.badge}">${q.status}</span></td>
        </tr>
    `).join('');
}

document.addEventListener('DOMContentLoaded', () => {
    populateQueue();

    // Auto-refresh
    const REFRESH_MS = 10000;
    setInterval(() => {
        populateQueue();
        const waitTimeEl = document.getElementById('waitTime');
        if (waitTimeEl) {
            let current = parseInt(waitTimeEl.textContent.replace(/\D/g, ''), 10);
            if (!isNaN(current)) waitTimeEl.textContent = Math.max(1, current - 1);
        }
    }, REFRESH_MS);

    // Refresh button
    document.getElementById('refreshBtn').addEventListener('click', () => {
        location.reload();
    });

    // Simulate ready button
    document.getElementById('simulateReadyBtn').addEventListener('click', () => {
        showReadyModal();
    });

    // New order button
    document.getElementById('newOrderBtn').addEventListener('click', () => {
        // Clear the current order and any selected items
        localStorage.removeItem('lastOrder');
        localStorage.removeItem('selectedItems');

        // Go back to the main ordering page
        window.location.href = '../index.html';
    });
});
