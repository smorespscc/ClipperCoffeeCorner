document.addEventListener('DOMContentLoaded', () => {
    // --- Time-based Menu Management ---
    const urlParams = new URLSearchParams(window.location.search);
    const viewMode = urlParams.get('view'); // 'breakfast' or 'lunch' for viewing mode
    const isViewingMode = viewMode !== null;
    
    // Check for debug mode
    const isDebugMode = localStorage.getItem('debugMode') === 'true';
    
    // Get current Pacific Time
    function getPacificTime() {
        const now = new Date();
        const pacificTime = new Date(now.toLocaleString('en-US', { timeZone: 'America/Los_Angeles' }));
        return pacificTime;
    }
    
    function getCurrentHourMinute() {
        const pt = getPacificTime();
        return pt.getHours() * 60 + pt.getMinutes(); // Convert to minutes since midnight
    }
    
    function determineServicePeriod() {
        if (isDebugMode) {
            return 'breakfast'; // Default to breakfast in debug mode
        }
        
        if (isViewingMode) {
            return viewMode; // 'breakfast' or 'lunch'
        }
        
        const currentMinutes = getCurrentHourMinute();
        const breakfastStart = 8 * 60; // 8:00 AM
        const breakfastEnd = 10 * 60 + 30; // 10:30 AM
        const lunchEnd = 14 * 60; // 2:00 PM
        
        if (currentMinutes >= breakfastStart && currentMinutes < breakfastEnd) {
            return 'breakfast';
        } else if (currentMinutes >= breakfastEnd && currentMinutes < lunchEnd) {
            return 'lunch';
        } else {
            return 'closed';
        }
    }
    
    const servicePeriod = determineServicePeriod();
    
    // Redirect to closed page if outside service hours and not in viewing mode or debug mode
    if (servicePeriod === 'closed' && !isViewingMode && !isDebugMode) {
        window.location.href = '/Home/Closed';
        return;
    }
    
    // Update header based on service period
    const servingStatus = document.getElementById('servingStatus');
    const servingStatusBadge = document.querySelector('.serving-status-badge');
    const servingType = document.getElementById('servingType');
    const servingHours = document.getElementById('servingHours');
    const viewingModeBanner = document.getElementById('viewingModeBanner');
    const specialRequestsSection = document.querySelector('.row.gx-3.gy-3.mb-2:last-child');
    const menuFooter = document.querySelector('.menu-footer');
    
    if (isViewingMode) {
        // Show viewing mode banner
        viewingModeBanner.style.display = 'block';
        servingStatus.textContent = 'Viewing';
        
        // Hide special requests and footer buttons
        if (specialRequestsSection) specialRequestsSection.style.display = 'none';
        if (menuFooter) menuFooter.style.display = 'none';
    } else {
        servingStatus.textContent = 'Currently Serving';
    }
    
    // Track current displayed menu (can be different from actual service period if user switches)
    let currentDisplayedMenu = servicePeriod;
    
    // Function to update header display
    function updateHeaderDisplay(menuType, isActuallyActive) {
        const servingIcon = document.getElementById('servingIcon');
        
        if (menuType === 'breakfast') {
            servingType.textContent = 'Breakfast';
            servingHours.textContent = '8:00am - 10:30am';
            if (servingIcon) {
                servingIcon.innerHTML = '<i class="bi bi-sunrise-fill"></i>';
            }
        } else if (menuType === 'lunch') {
            servingType.textContent = 'Lunch';
            servingHours.textContent = '10:30am - 2:00pm';
            if (servingIcon) {
                servingIcon.innerHTML = '<i class="bi bi-sun-fill"></i>';
            }
        }
        
        // Update serving status based on whether the displayed menu is active
        if (isActuallyActive && !isViewingMode) {
            servingStatus.textContent = 'Currently Serving';
            if (servingStatusBadge) {
                servingStatusBadge.classList.remove('unavailable');
            }
        } else {
            servingStatus.textContent = 'Menu Unavailable';
            if (servingStatusBadge) {
                servingStatusBadge.classList.add('unavailable');
            }
        }
        
        // Update dropdown menu items
        document.querySelectorAll('.menu-option').forEach(option => {
            const optionMenu = option.getAttribute('data-menu');
            option.classList.remove('active', 'inactive');
            
            if (optionMenu === servicePeriod && !isViewingMode) {
                option.classList.add('active');
            } else {
                option.classList.add('inactive');
            }
        });
    }
    
    // Initial header update
    updateHeaderDisplay(servicePeriod, true);
    
    // Define which items belong to which service period
    const breakfastItems = [
        'Cappuccino', 'Vanilla Latte', 'House Brew', 'Chai Tea Latte', 'Espresso', 
        'Americano', 'Macchiato', 'Flat White', 'Cortado', 'Double Espresso',
        'Butter Croissant', 'Blueberry Muffin', 'Pancake Stack', 'Cheese Omelette',
        'French Toast', 'Breakfast Sandwich', 'Bagel & Cream Cheese', 'Belgian Waffle',
        'Eggs Benedict', 'Breakfast Bowl', 'Breakfast Burrito'
    ];
    
    const lunchItems = [
        'Iced Mocha', 'Chai Tea Latte', 'House Brew', 'Americano', 'Espresso',
        'Avocado Toast', 'Breakfast Sandwich', 'Bagel & Cream Cheese'
    ];
    
    // Function to update menu items availability and reorder carousels
    function updateMenuItemsAvailability(currentPeriod) {
        document.querySelectorAll('.menu-item').forEach(img => {
            const itemName = img.getAttribute('data-item') || img.alt;
            const wrapper = img.closest('.carousel-item');
            
            if (!wrapper) return;
            
            let isAvailable = false;
            let availabilityMessage = '';
            
            // In debug mode, all items are available
            if (isDebugMode) {
                isAvailable = true;
            } else {
                if (currentPeriod === 'breakfast') {
                    isAvailable = breakfastItems.includes(itemName);
                    availabilityMessage = 'Available during Lunch hours';
                } else if (currentPeriod === 'lunch') {
                    isAvailable = lunchItems.includes(itemName);
                    availabilityMessage = 'Available during Breakfast hours';
                }
            }
            
            // Remove previous disabled state
            wrapper.classList.remove('disabled');
            wrapper.removeAttribute('data-availability-message');
            img.setAttribute('data-bs-toggle', 'modal');
            img.setAttribute('data-bs-target', '#itemModal');
            img.style.cursor = 'pointer';
            
            if ((!isAvailable || isViewingMode) && !isDebugMode) {
                wrapper.classList.add('disabled');
                wrapper.setAttribute('data-availability-message', availabilityMessage);
                // Remove modal trigger attributes
                img.removeAttribute('data-bs-toggle');
                img.removeAttribute('data-bs-target');
                img.style.cursor = 'not-allowed';
            }
        });
        
        // Reorder carousel items to show available items first (skip in debug mode)
        if (!isDebugMode) {
            reorderCarouselItems(currentPeriod);
        }
    }
    
    // Function to reorder carousel items based on availability
    function reorderCarouselItems(currentPeriod) {
        document.querySelectorAll('.carousel').forEach(carousel => {
            const carouselInner = carousel.querySelector('.carousel-inner');
            if (!carouselInner) return;
            
            const items = Array.from(carouselInner.querySelectorAll('.carousel-item'));
            if (items.length === 0) return;
            
            // Separate available and unavailable items
            const availableItems = [];
            const unavailableItems = [];
            
            items.forEach(item => {
                const img = item.querySelector('.menu-item');
                if (!img) return;
                
                const itemName = img.getAttribute('data-item') || img.alt;
                let isAvailable = false;
                
                if (currentPeriod === 'breakfast') {
                    isAvailable = breakfastItems.includes(itemName);
                } else if (currentPeriod === 'lunch') {
                    isAvailable = lunchItems.includes(itemName);
                }
                
                if (isAvailable) {
                    availableItems.push(item);
                } else {
                    unavailableItems.push(item);
                }
            });
            
            // Clear carousel and reorder: available items first, then unavailable
            carouselInner.innerHTML = '';
            const reorderedItems = [...availableItems, ...unavailableItems];
            
            reorderedItems.forEach((item, index) => {
                // Set first item as active
                if (index === 0) {
                    item.classList.add('active');
                } else {
                    item.classList.remove('active');
                }
                carouselInner.appendChild(item);
            });
        });
    }
    
    // Initial update
    updateMenuItemsAvailability(servicePeriod);
    
    // --- Helpers ---
    function getSelectedItems() { return JSON.parse(localStorage.getItem('selectedItems') || '[]'); }
    function saveSelectedItems(items) { localStorage.setItem('selectedItems', JSON.stringify(items)); }
    
    // --- Applied Saved Orders Management ---
    function getAppliedSavedOrders() { return JSON.parse(localStorage.getItem('appliedSavedOrders') || '[]'); }
    function saveAppliedSavedOrders(orders) { localStorage.setItem('appliedSavedOrders', JSON.stringify(orders)); }
    
    // --- Saved Orders Management ---
    function getSavedOrders() { return JSON.parse(localStorage.getItem('savedOrders') || '[]'); }
    function saveSavedOrders(orders) { localStorage.setItem('savedOrders', JSON.stringify(orders)); }
    
    // --- Recent Orders Management ---
    function getRecentOrders() { return JSON.parse(localStorage.getItem('recentOrders') || '[]'); }
    function saveRecentOrders(orders) { localStorage.setItem('recentOrders', JSON.stringify(orders)); }
    
    // --- Add order to recent orders (called from payment success) ---
    function addToRecentOrders(orderData) {
        const recentOrders = getRecentOrders();
        const orderWithTimestamp = {
            ...orderData,
            timestamp: Date.now()
        };
        
        // Add to beginning of array
        recentOrders.unshift(orderWithTimestamp);
        
        // Keep only last 10 recent orders
        if (recentOrders.length > 10) {
            recentOrders.splice(10);
        }
        
        saveRecentOrders(recentOrders);
    }
    
    // --- Save current order configuration ---
    function saveCurrentOrder() {
        if (!currentItemName || tabInstances.length === 0) {
            showNotification('No order to save. Please customize an item first.', 'error');
            return;
        }
        
        // Save current tab state
        saveCurrentTabState();
        
        const savedOrders = getSavedOrders();
        
        let orderName;
        if (currentEditingSavedOrderId) {
            // Find existing order name
            const existingOrder = savedOrders.find(o => o.id === currentEditingSavedOrderId);
            orderName = prompt('Enter a name for this saved order:', existingOrder ? existingOrder.name : `${currentItemName} Order`);
        } else {
            orderName = prompt('Enter a name for this saved order:', `${currentItemName} Order`);
        }
        
        if (!orderName || !orderName.trim()) {
            return;
        }
        
        // Check if updating existing saved order
        if (currentEditingSavedOrderId) {
            const orderIndex = savedOrders.findIndex(o => o.id === currentEditingSavedOrderId);
            if (orderIndex !== -1) {
                savedOrders[orderIndex] = {
                    id: currentEditingSavedOrderId,
                    name: orderName.trim(),
                    itemName: currentItemName,
                    itemType: currentItemType,
                    tabs: tabInstances.map(tab => ({
                        modifiers: [...(tab.modifiers || [])],
                        specialRequests: tab.specialRequests || ''
                    })),
                    savedAt: Date.now()
                };
                saveSavedOrders(savedOrders);
                showNotification(`Order "${orderName.trim()}" has been updated!`, 'success');
            }
        } else {
            // Create new saved order
            const savedOrder = {
                id: Date.now(),
                name: orderName.trim(),
                itemName: currentItemName,
                itemType: currentItemType,
                tabs: tabInstances.map(tab => ({
                    modifiers: [...(tab.modifiers || [])],
                    specialRequests: tab.specialRequests || ''
                })),
                savedAt: Date.now()
            };
            
            savedOrders.push(savedOrder);
            saveSavedOrders(savedOrders);
            showNotification(`Order "${orderName.trim()}" has been saved!`, 'success');
        }
        
        // Refresh the saved orders carousel
        populateSavedOrdersCarousel();
        
        // Close the modal immediately after saving
        const modalInstance = bootstrap.Modal.getInstance(itemModalEl);
        if (modalInstance) modalInstance.hide();
    }
    
    // --- Load saved order ---
    function loadSavedOrder(savedOrder) {
        if (!savedOrder || !savedOrder.tabs) return;
        
        // Set current item info
        currentItemName = savedOrder.itemName;
        currentItemType = savedOrder.itemType;
        
        // Show/hide appropriate options based on item type
        if (currentItemType === 'food') {
            drinkOptions.style.display = 'none';
            foodOptions.style.display = 'block';
        } else {
            drinkOptions.style.display = 'block';
            foodOptions.style.display = 'none';
        }
        
        // Recreate tab instances from saved data
        tabInstances = [];
        tabIdCounter = 0;
        
        savedOrder.tabs.forEach(tabData => {
            tabInstances.push({
                id: `tab-${++tabIdCounter}`,
                modifiers: [...(tabData.modifiers || [])],
                specialRequests: tabData.specialRequests || ''
            });
        });
        
        if (tabInstances.length > 0) {
            activeTabId = tabInstances[0].id;
            renderTabs();
            
            // Load first tab state
            setTimeout(() => {
                if (activeTabId) {
                    const firstTab = tabInstances.find(t => t.id === activeTabId);
                    if (firstTab) {
                        // Update tab UI
                        if (instanceTabsContainer) {
                            instanceTabsContainer.querySelectorAll('.instance-tab').forEach(tab => {
                                tab.classList.toggle('active', tab.dataset.tabId === activeTabId);
                            });
                        }
                        loadTabState(firstTab);
                    }
                }
                adjustTabWidths();
            }, 50);
        }
    }
    
    // --- Load recent order into modal ---
    function loadRecentOrderIntoModal(recentOrder) {
        if (!recentOrder || !recentOrder.items || recentOrder.items.length === 0) return;
        
        // Find the first item that matches current item name/type
        const matchingItem = recentOrder.items.find(item => 
            item.name === currentItemName && item.type === currentItemType
        );
        
        if (!matchingItem) return;
        
        // Show/hide appropriate options based on item type
        if (currentItemType === 'food') {
            drinkOptions.style.display = 'none';
            foodOptions.style.display = 'block';
        } else {
            drinkOptions.style.display = 'block';
            foodOptions.style.display = 'none';
        }
        
        // Create tabs based on quantity
        tabInstances = [];
        tabIdCounter = 0;
        
        const quantity = matchingItem.quantity || 1;
        for (let i = 0; i < quantity; i++) {
            tabInstances.push({
                id: `tab-${++tabIdCounter}`,
                modifiers: Array.isArray(matchingItem.modifiers) ? [...matchingItem.modifiers] : [],
                specialRequests: matchingItem.specialRequests || ''
            });
        }
        
        if (tabInstances.length > 0) {
            activeTabId = tabInstances[0].id;
            renderTabs();
            
            setTimeout(() => {
                if (activeTabId) {
                    const firstTab = tabInstances.find(t => t.id === activeTabId);
                    if (firstTab) {
                        if (instanceTabsContainer) {
                            instanceTabsContainer.querySelectorAll('.instance-tab').forEach(tab => {
                                tab.classList.toggle('active', tab.dataset.tabId === activeTabId);
                            });
                        }
                        loadTabState(firstTab);
                    }
                }
                adjustTabWidths();
            }, 50);
        }
    }

 
let currentItemName = null;
let currentItemType = null;
let currentTriggerImg = null;
let wasAlreadySelected = false;
let currentSelectedItemData = null;
let cancelButtonClicked = false;
let currentEditingSavedOrderId = null;
let isLoadingFromSavedOrRecent = false;

// --- Notification System ---
function showNotification(message, type = 'info') {
    const existingNotification = document.querySelector('.custom-notification');
    if (existingNotification) {
        existingNotification.remove();
    }
    
    const notification = document.createElement('div');
    notification.className = `custom-notification custom-notification-${type}`;
    notification.innerHTML = `
        <div class="custom-notification-content">
            <i class="bi ${type === 'success' ? 'bi-check-circle-fill' : type === 'error' ? 'bi-x-circle-fill' : 'bi-info-circle-fill'}"></i>
            <span>${message}</span>
        </div>
    `;
    
    document.body.appendChild(notification);
    setTimeout(() => notification.classList.add('show'), 10);
    
    setTimeout(() => {
        notification.classList.remove('show');
        setTimeout(() => notification.remove(), 300);
    }, 3000);
}

// --- Update Modal Footer Buttons ---
function updateModalFooterButtons() {
    const modalFooter = itemModalEl.querySelector('.modal-footer');
    const modalCancelBtn = modalFooter.querySelector('.btn-cancel');
    const saveOrderBtn = document.getElementById('saveOrderBtn');
    const applyBtn = document.getElementById('applyModifiers');
    
    // Reset to default state first
    if (modalCancelBtn) {
        modalCancelBtn.textContent = 'Cancel';
        modalCancelBtn.className = 'btn btn-secondary';
        modalCancelBtn.onclick = null;
        modalCancelBtn.setAttribute('data-bs-dismiss', 'modal');
    }
    
    if (saveOrderBtn) {
        saveOrderBtn.textContent = 'Save Order';
        saveOrderBtn.className = 'btn btn-secondary';
        saveOrderBtn.style.display = 'inline-block';
        saveOrderBtn.onclick = () => {
            saveCurrentOrder();
        };
    }
    
    if (applyBtn) {
        applyBtn.textContent = 'Apply';
        applyBtn.className = 'btn btn-order';
        applyBtn.style.display = 'inline-block';
    }
    
    // If editing a saved order, hide Save Order button and change Apply to Update
    if (currentEditingSavedOrderId) {
        if (modalCancelBtn) {
            modalCancelBtn.textContent = 'Cancel';
            modalCancelBtn.className = 'btn btn-secondary';
            modalCancelBtn.setAttribute('data-bs-dismiss', 'modal');
            modalCancelBtn.onclick = null;
        }
        
        // Hide the Save Order button when editing a saved order
        if (saveOrderBtn) {
            saveOrderBtn.style.display = 'none';
        }
        
        if (applyBtn) {
            applyBtn.textContent = 'Update';
            applyBtn.className = 'btn btn-primary';
        }
    }
}
const itemModalEl = document.getElementById('itemModal');
const applyBtn = document.getElementById('applyModifiers');
const orderBtn = document.getElementById('orderBtn');
const specialInput = document.getElementById('specialRequests');
const hiddenSpecial = document.getElementById('hiddenSpecialRequests');
const micButton = document.getElementById('micButton');
const drinkOptions = document.getElementById('drinkOptions');
const foodOptions = document.getElementById('foodOptions');
const instanceTabsContainer = document.getElementById('instanceTabs');
const addTabBtn = document.getElementById('addInstanceTab');

if (!itemModalEl || !applyBtn || !orderBtn || !drinkOptions || !foodOptions) return; // safety check

// Tab management state
let tabInstances = []; // Array of {id, modifiers, specialRequests}
let activeTabId = null;
let tabIdCounter = 0;

// Mapping between checkbox IDs and their label text (for restoring checkboxes)
const checkboxIdToLabelMap = {
    // Drink options
    'hot': 'Hot',
    'iced': 'Iced',
    'dairy': 'Dairy',
    'oat': 'Oat',
    'almond': 'Almond',
    'soy': 'Soy',
    'vanilla': 'Vanilla',
    'mint': 'Mint',
    'cinnamon': 'Cinnamon',
    'hazelnut': 'Hazelnut',
    'caramel': 'Caramel',
    'whip': 'Whip Cream',
    'extraShot': 'Extra Shot',
    'decaf': 'Decaf',
    'sugarFree': 'Sugar Free',
    'caffeineFree': 'Caffeine Free',
    // Food options
    'wellDone': 'Well Done',
    'medium': 'Medium',
    'light': 'Light',
    'hashBrowns': 'Hash Browns',
    'fruit': 'Fresh Fruit',
    'toast': 'Toast',
    'extraCheese': 'Extra Cheese',
    'bacon': 'Bacon',
    'avocado': 'Avocado',
    'sausage': 'Sausage',
    'glutenFree': 'Gluten Free',
    'vegetarian': 'Vegetarian',
    'vegan': 'Vegan',
    'noDairy': 'No Dairy',
    'noOnions': 'No Onions',
    'noTomatoes': 'No Tomatoes',
    'extraCrispy': 'Extra Crispy'
};

// --- Helper function to normalize item properties for comparison ---
function normalizeItemForComparison(item) {
    // Sort modifiers array for consistent comparison
    const modifiers = Array.isArray(item.modifiers) 
        ? [...item.modifiers].sort().join('|') 
        : '';
    const specialRequests = (item.specialRequests || '').trim().toLowerCase();
    
    return {
        name: item.name,
        type: item.type,
        modifiers: modifiers,
        specialRequests: specialRequests
    };
}

// --- Helper function to check if two items are identical (all properties match) ---
function areItemsIdentical(item1, item2) {
    const norm1 = normalizeItemForComparison(item1);
    const norm2 = normalizeItemForComparison(item2);
    
    return norm1.name === norm2.name &&
           norm1.type === norm2.type &&
           norm1.modifiers === norm2.modifiers &&
           norm1.specialRequests === norm2.specialRequests;
}

// --- Helper function to check if an item with same name/type exists (regardless of properties) ---
function hasItemWithSameNameAndType(itemName, itemType) {
    const items = getSelectedItems();
    return items.some(item => item.name === itemName && item.type === itemType);
}

// --- Helper function to find an identical item (all properties match) ---
function findIdenticalItem(newItem) {
    const items = getSelectedItems();
    return items.findIndex(item => areItemsIdentical(item, newItem));
}

// --- Helper function to remove a specific item by index ---
function removeSelectedItemByIndex(index) {
    const items = getSelectedItems();
    items.splice(index, 1);
    saveSelectedItems(items);
}

// --- Helper function to remove visual indicators ---
function removeVisualIndicators(imgElement) {
    if (!imgElement) return;
    const wrapper = imgElement.closest('.carousel-item');
    if (wrapper) {
        wrapper.classList.remove('selected');
        const indicator = wrapper.querySelector('.selected-indicator');
        if (indicator) {
            indicator.remove();
        }
    }
}

// --- Helper function to get selected item data (first match by name/type) ---
function getSelectedItemData(itemName, itemType) {
    const items = getSelectedItems();
    return items.find(item => item.name === itemName && item.type === itemType) || null;
}

// --- Helper function to get total quantity for an item (all variations) ---
function getItemTotalQuantity(itemName, itemType) {
    const items = getSelectedItems();
    return items
        .filter(item => item.name === itemName && item.type === itemType)
        .reduce((sum, item) => sum + (item.quantity || 1), 0);
}

// --- Helper function to restore checkbox states from saved modifiers ---
function restoreCheckboxStates(modifiers) {
    if (!modifiers || !Array.isArray(modifiers)) {
        // Reset all checkboxes if no modifiers
        document.querySelectorAll('#itemModal input[type=checkbox]').forEach(cb => cb.checked = false);
        return;
    }
    
    // Reset all checkboxes first
    document.querySelectorAll('#itemModal input[type=checkbox]').forEach(cb => cb.checked = false);
    
    // Check each checkbox based on saved modifiers
    // Modifiers are stored as label text, so we need to match them
    Object.keys(checkboxIdToLabelMap).forEach(checkboxId => {
        const labelText = checkboxIdToLabelMap[checkboxId];
        const checkbox = document.getElementById(checkboxId);
        
        if (checkbox && modifiers.includes(labelText)) {
            checkbox.checked = true;
        }
    });
}

// --- Tab Management Functions ---
function createTabInstance() {
    const id = `tab-${++tabIdCounter}`;
    const instance = {
        id: id,
        modifiers: [],
        specialRequests: ''
    };
    tabInstances.push(instance);
    return instance;
}

function createTabElement(instance, index) {
    const tab = document.createElement('div');
    tab.className = `instance-tab ${index === 0 ? 'active' : ''}`;
    tab.dataset.tabId = instance.id;
    
    const label = document.createElement('span');
    label.className = 'instance-tab-label';
    label.textContent = `${currentItemName} #${index + 1}`;
    
    const closeBtn = document.createElement('button');
    closeBtn.className = 'instance-tab-close';
    closeBtn.type = 'button';
    closeBtn.innerHTML = '<i class="bi bi-x"></i>';
    closeBtn.onclick = (e) => {
        e.stopPropagation();
        closeTab(instance.id);
    };
    
    tab.appendChild(label);
    if (index > 0) { // First tab cannot be closed
        tab.appendChild(closeBtn);
    }
    
    tab.onclick = () => switchToTab(instance.id);
    
    return tab;
}

function renderTabs() {
    if (!instanceTabsContainer) return;
    
    instanceTabsContainer.innerHTML = '';
    tabInstances.forEach((instance, index) => {
        const tabElement = createTabElement(instance, index);
        instanceTabsContainer.appendChild(tabElement);
    });
    
    // Adjust tab widths
    adjustTabWidths();
}

function adjustTabWidths() {
    if (!instanceTabsContainer) return;
    const tabs = instanceTabsContainer.querySelectorAll('.instance-tab');
    const tabCount = tabs.length;
    if (tabCount === 0) return;
    
    // Calculate available width (subtract add button and gaps)
    const container = instanceTabsContainer.parentElement;
    if (!container) return;
    
    const addBtnWidth = 36 + 8; // button width + gap
    const gaps = (tabCount - 1) * 8; // gap between tabs
    const padding = 32; // container padding
    const availableWidth = container.offsetWidth - addBtnWidth - gaps - padding;
    
    // Set max width per tab (with min and max constraints)
    const maxWidth = Math.max(80, Math.min(200, Math.floor(availableWidth / tabCount)));
    tabs.forEach(tab => {
        tab.style.maxWidth = `${maxWidth}px`;
    });
}

// Handle window resize for tab width adjustment
let resizeTimeout;
window.addEventListener('resize', () => {
    clearTimeout(resizeTimeout);
    resizeTimeout = setTimeout(() => {
        if (itemModalEl && itemModalEl.classList.contains('show')) {
            adjustTabWidths();
        }
    }, 150);
});

function switchToTab(tabId) {
    // Save current tab state before switching
    if (activeTabId && activeTabId !== tabId) {
        saveCurrentTabState();
    }
    
    // Find and activate the tab
    const tabInstance = tabInstances.find(t => t.id === tabId);
    if (!tabInstance) return;
    
    activeTabId = tabId;
    
    // Update tab UI
    if (instanceTabsContainer) {
        instanceTabsContainer.querySelectorAll('.instance-tab').forEach(tab => {
            tab.classList.toggle('active', tab.dataset.tabId === tabId);
        });
    }
    
    // Load tab state into form
    loadTabState(tabInstance);
}

function loadTabState(instance) {
    if (!instance) return;
    
    // Ensure modifiers is an array and make a copy to avoid reference issues
    const modifiers = Array.isArray(instance.modifiers) ? [...instance.modifiers] : [];
    
    // Restore checkboxes - this will reset all checkboxes first, then check the ones in modifiers
    restoreCheckboxStates(modifiers);
    
    // Restore special requests
    const specialRequests = instance.specialRequests || '';
    if (specialInput) {
        specialInput.value = specialRequests;
        if (hiddenSpecial) {
            hiddenSpecial.value = specialRequests;
        }
    }
}

function saveCurrentTabState() {
    if (!activeTabId) return;
    
    const instance = tabInstances.find(t => t.id === activeTabId);
    if (!instance) return;
    
    // Save modifiers from the visible section
    const visibleSection = currentItemType === 'food' ? foodOptions : drinkOptions;
    if (visibleSection) {
        instance.modifiers = Array.from(visibleSection.querySelectorAll('input[type=checkbox]:checked'))
            .map(cb => {
                const label = cb.nextElementSibling;
                return label ? label.textContent.trim() : cb.id;
            });
    } else {
        instance.modifiers = [];
    }
    
    // Save special requests
    instance.specialRequests = specialInput?.value || '';
}

function addNewTab() {
    saveCurrentTabState();
    const newInstance = createTabInstance();
    renderTabs();
    switchToTab(newInstance.id);
}

function closeTab(tabId) {
    if (tabInstances.length <= 1) return; // Cannot close last tab
    
    const index = tabInstances.findIndex(t => t.id === tabId);
    if (index === -1) return;
    
    tabInstances.splice(index, 1);
    
    // If closing active tab, switch to another
    if (activeTabId === tabId) {
        const newActiveIndex = Math.min(index, tabInstances.length - 1);
        activeTabId = tabInstances[newActiveIndex].id;
    }
    
    renderTabs();
    if (activeTabId) {
        switchToTab(activeTabId);
    }
}

function initializeTabs() {
    tabInstances = [];
    tabIdCounter = 0;
    
    // Create first tab
    const firstTab = createTabInstance();
    activeTabId = firstTab.id;
    renderTabs();
    
    // Reset form
    document.querySelectorAll('#itemModal input[type=checkbox]').forEach(cb => cb.checked = false);
    if (specialInput) {
        specialInput.value = '';
        if (hiddenSpecial) {
            hiddenSpecial.value = '';
        }
    }
}

// --- Modal show/hide ---
itemModalEl.addEventListener('show.bs.modal', (event) => {
    const trigger = event.relatedTarget;
    cancelButtonClicked = false;
    
    if (trigger) {
        currentItemName = trigger.getAttribute('data-item') || trigger.alt || 'Item';
        currentItemType = trigger.getAttribute('data-type') || 'drink';
        currentTriggerImg = trigger;
        
        // Check if loading from saved/recent orders
        isLoadingFromSavedOrRecent = trigger.classList.contains('saved-order-item') || 
                                      trigger.classList.contains('recent-order-item');
        
        // Check if this item is already selected (any variation)
        wasAlreadySelected = hasItemWithSameNameAndType(currentItemName, currentItemType) && !isLoadingFromSavedOrRecent;
        
        // Update modal title
        const modalTitle = document.getElementById('itemModalLabel');
        
        if (isLoadingFromSavedOrRecent) {
            if (trigger.classList.contains('saved-order-item')) {
                modalTitle.innerHTML = 'Customize ' + currentItemName + ' <span class="badge bg-warning text-dark ms-2">Editing Saved Order</span>';
            } else {
                modalTitle.innerHTML = 'Customize ' + currentItemName + ' <span class="badge bg-info text-white ms-2">From Recent Orders</span>';
            }
        } else if (wasAlreadySelected) {
            modalTitle.innerHTML = 'Customize ' + currentItemName + ' <span class="badge bg-info text-white ms-2">Editing Selected Item</span>';
        } else {
            modalTitle.textContent = 'Customize ' + currentItemName;
        }
        
        // Show/hide appropriate options based on item type
        if (currentItemType === 'food') {
            drinkOptions.style.display = 'none';
            foodOptions.style.display = 'block';
        } else {
            drinkOptions.style.display = 'block';
            foodOptions.style.display = 'none';
        }
    }

    // Update modal footer buttons based on context
    updateModalFooterButtons();

    // Hide carousel arrows
    [...document.getElementsByClassName('carousel-control-prev')].forEach(btn => btn.style.display = 'none');
    [...document.getElementsByClassName('carousel-control-next')].forEach(btn => btn.style.display = 'none');

    // Check if there's a pending saved order to load (for editing)
    if (window.pendingSavedOrder) {
        const savedOrder = window.pendingSavedOrder;
        currentEditingSavedOrderId = savedOrder.id;
        window.pendingSavedOrder = null;
        
        loadSavedOrder(savedOrder);
        return;
    }
    
    // Check if there's a pending recent order to load
    if (window.pendingRecentOrder) {
        const recentOrder = window.pendingRecentOrder;
        window.pendingRecentOrder = null;
        
        loadRecentOrderIntoModal(recentOrder);
        return;
    }

    // Initialize tabs - restore existing instances or create new
    const existingItems = getSelectedItems().filter(item => 
        item.name === currentItemName && item.type === currentItemType
    );
    
    if (existingItems.length > 0 && !isLoadingFromSavedOrRecent) {
        // Restore existing instances as tabs
        tabInstances = [];
        existingItems.forEach((item) => {
            const quantity = item.quantity || 1;
            for (let i = 0; i < quantity; i++) {
                tabInstances.push({
                    id: `tab-${++tabIdCounter}`,
                    modifiers: Array.isArray(item.modifiers) ? [...item.modifiers] : [],
                    specialRequests: item.specialRequests || ''
                });
            }
        });
        
        activeTabId = tabInstances[0].id;
        renderTabs();
        
        setTimeout(() => {
            if (tabInstances.length > 0 && activeTabId) {
                const firstTab = tabInstances.find(t => t.id === activeTabId);
                if (firstTab) {
                    activeTabId = firstTab.id;
                    if (instanceTabsContainer) {
                        instanceTabsContainer.querySelectorAll('.instance-tab').forEach(tab => {
                            tab.classList.toggle('active', tab.dataset.tabId === activeTabId);
                        });
                    }
                    loadTabState(firstTab);
                }
            }
            adjustTabWidths();
        }, 50);
    } else {
        // For unselected items, start fresh with cleared checkboxes
        initializeTabs();
        setTimeout(() => adjustTabWidths(), 100);
    }
});

itemModalEl.addEventListener('hidden.bs.modal', (event) => {
    // Show carousel arrows again
    [...document.getElementsByClassName('carousel-control-prev')].forEach(btn => btn.style.display = 'flex');
    [...document.getElementsByClassName('carousel-control-next')].forEach(btn => btn.style.display = 'flex');
    
    // Clear all tabs and reset modal state
    tabInstances = [];
    activeTabId = null;
    if (instanceTabsContainer) {
        instanceTabsContainer.innerHTML = '';
    }
    
    // Reset all checkboxes
    document.querySelectorAll('#itemModal input[type=checkbox]').forEach(cb => cb.checked = false);
    
    // Clear special requests input
    if (specialInput) {
        specialInput.value = '';
        if (hiddenSpecial) {
            hiddenSpecial.value = '';
        }
    }
    
    // Reset tracking
    wasAlreadySelected = false;
    currentItemName = null;
    currentItemType = null;
    currentTriggerImg = null;
    currentSelectedItemData = null;
    cancelButtonClicked = false;
    currentEditingSavedOrderId = null;
    isLoadingFromSavedOrRecent = false;
    
    // Reset modal footer buttons to default state
    const modalFooter = itemModalEl.querySelector('.modal-footer');
    const modalCancelBtn = modalFooter.querySelector('.btn-cancel');
    const saveOrderBtn = document.getElementById('saveOrderBtn');
    const applyBtn = document.getElementById('applyModifiers');
    
    if (modalCancelBtn) {
        modalCancelBtn.textContent = 'Cancel';
        modalCancelBtn.className = 'btn btn-cancel';
        modalCancelBtn.onclick = null;
        modalCancelBtn.setAttribute('data-bs-dismiss', 'modal');
    }
    
    if (saveOrderBtn) {
        saveOrderBtn.textContent = 'Save Order';
        saveOrderBtn.style.display = 'inline-block';
    }
    
    if (applyBtn) {
        applyBtn.textContent = 'Apply';
        applyBtn.style.display = 'inline-block';
    }
});

// --- Add Tab Button ---
if (addTabBtn) {
    addTabBtn.addEventListener('click', addNewTab);
}

// --- Save Order Button ---
const saveOrderBtn = document.getElementById('saveOrderBtn');
if (saveOrderBtn) {
    saveOrderBtn.addEventListener('click', () => {
        saveCurrentOrder();
    });
}

// --- Apply modifiers ---
applyBtn.addEventListener('click', () => {
    if (!currentItemName) {
        showNotification("Please select an item before applying modifiers.", 'error');
        return;
    }

    // Save current tab state
    saveCurrentTabState();
    
    const items = getSelectedItems();
    const appliedSavedOrders = getAppliedSavedOrders();
    
    // If editing a saved order and user clicks "Update", just update the saved order
    if (currentEditingSavedOrderId) {
        const savedOrders = getSavedOrders();
        const orderIndex = savedOrders.findIndex(o => o.id === currentEditingSavedOrderId);
        if (orderIndex !== -1) {
            savedOrders[orderIndex].tabs = tabInstances.map(tab => ({
                modifiers: [...(tab.modifiers || [])],
                specialRequests: tab.specialRequests || ''
            }));
            savedOrders[orderIndex].savedAt = Date.now();
            saveSavedOrders(savedOrders);
            
            // Check if this saved order is currently applied
            const isCurrentlyApplied = appliedSavedOrders.some(applied => applied.id === currentEditingSavedOrderId);
            
            if (isCurrentlyApplied) {
                // Remove old instances of this saved order from cart
                const filteredItems = items.filter(item => item.fromSavedOrder !== currentEditingSavedOrderId);
                const filteredAppliedOrders = appliedSavedOrders.filter(applied => applied.id !== currentEditingSavedOrderId);
                
                // Add updated saved order items
                tabInstances.forEach(instance => {
                    const itemObj = {
                        name: currentItemName,
                        type: currentItemType,
                        modifiers: instance.modifiers || [],
                        specialRequests: instance.specialRequests || '',
                        quantity: 1,
                        fromSavedOrder: currentEditingSavedOrderId
                    };
                    
                    const identicalIndex = filteredItems.findIndex(item => areItemsIdentical(item, itemObj));
                    if (identicalIndex !== -1) {
                        filteredItems[identicalIndex].quantity = (filteredItems[identicalIndex].quantity || 1) + 1;
                    } else {
                        filteredItems.push(itemObj);
                    }
                });
                
                // Update applied saved orders
                filteredAppliedOrders.push({
                    id: currentEditingSavedOrderId,
                    name: savedOrders[orderIndex].name,
                    itemName: currentItemName,
                    itemType: currentItemType,
                    totalQuantity: tabInstances.length,
                    appliedAt: Date.now()
                });
                
                saveSelectedItems(filteredItems);
                saveAppliedSavedOrders(filteredAppliedOrders);
                showNotification(`Saved order "${savedOrders[orderIndex].name}" has been updated!`, 'success');
            } else {
                showNotification(`Saved order "${savedOrders[orderIndex].name}" has been updated!`, 'success');
            }
            
            populateSavedOrdersCarousel();
        }
    } else if (isLoadingFromSavedOrRecent) {
        // For saved/recent orders, add to existing items without removing anything
        const filteredItems = [...items];
        
        tabInstances.forEach(instance => {
            const itemObj = {
                name: currentItemName,
                type: currentItemType,
                modifiers: instance.modifiers || [],
                specialRequests: instance.specialRequests || '',
                quantity: 1
            };
            
            const identicalIndex = filteredItems.findIndex(item => areItemsIdentical(item, itemObj));
            if (identicalIndex !== -1) {
                filteredItems[identicalIndex].quantity = (filteredItems[identicalIndex].quantity || 1) + 1;
            } else {
                filteredItems.push(itemObj);
            }
        });
        
        saveSelectedItems(filteredItems);
    } else {
        // For regular menu items, remove existing instances of this item and add new ones
        const filteredItems = items.filter(item => 
            !(item.name === currentItemName && item.type === currentItemType && !item.fromSavedOrder)
        );
        
        tabInstances.forEach(instance => {
            const itemObj = {
                name: currentItemName,
                type: currentItemType,
                modifiers: instance.modifiers || [],
                specialRequests: instance.specialRequests || '',
                quantity: 1
            };
            
            const identicalIndex = filteredItems.findIndex(item => areItemsIdentical(item, itemObj));
            if (identicalIndex !== -1) {
                filteredItems[identicalIndex].quantity = (filteredItems[identicalIndex].quantity || 1) + 1;
            } else {
                filteredItems.push(itemObj);
            }
        });
        
        saveSelectedItems(filteredItems);
    }

    // Update visual indicator with quantity badge
    // Only update visual indicators for regular menu items, not saved/recent orders
    if (currentTriggerImg && !isLoadingFromSavedOrRecent && !currentEditingSavedOrderId) {
        const wrapper = currentTriggerImg.closest('.carousel-item');
        if (wrapper) {
            // Remove existing indicator if any
            const existingIndicator = wrapper.querySelector('.selected-indicator');
            if (existingIndicator) {
                existingIndicator.remove();
            }
            
            // Add selected class to wrapper for border and overlay effects
            wrapper.classList.add('selected');
            
            // Get total quantity for this item (in case there are multiple entries)
            const totalQuantity = getItemTotalQuantity(currentItemName, currentItemType);
            
            // Create badge indicator with Bootstrap icon and quantity
            const indicator = document.createElement('div');
            indicator.className = 'selected-indicator';
            if (totalQuantity > 1) {
                indicator.innerHTML = `<i class="bi bi-check-circle-fill"></i><span class="quantity-badge">${totalQuantity}</span>`;
            } else {
                indicator.innerHTML = '<i class="bi bi-check-circle-fill"></i>';
            }
            
            // Add hover effect to turn checkmark into red X
            indicator.addEventListener('mouseenter', function() {
                this.classList.add('remove-mode');
                const icon = this.querySelector('i');
                if (icon) {
                    icon.className = 'bi bi-x-circle-fill';
                }
            });
            
            indicator.addEventListener('mouseleave', function() {
                this.classList.remove('remove-mode');
                const icon = this.querySelector('i');
                if (icon) {
                    icon.className = 'bi bi-check-circle-fill';
                }
            });
            
            // Add click handler to remove all instances of this item
            indicator.addEventListener('click', function(e) {
                e.stopPropagation();
                e.preventDefault();
                
                // Remove all instances of this item
                removeAllInstancesOfItem(currentItemName, currentItemType);
                
                // Remove visual indicator
                wrapper.classList.remove('selected');
                this.remove();
            });
            
            // Make indicator clickable
            indicator.style.pointerEvents = 'auto';
            indicator.style.cursor = 'pointer';
            
            wrapper.appendChild(indicator);
        }
    }
    
    // If loading from saved/recent orders, only update saved orders carousel, not regular menu items
    if (isLoadingFromSavedOrRecent || currentEditingSavedOrderId) {
        // Only refresh saved orders carousel to show indicators there
        populateSavedOrdersCarousel();
    } else {
        // For regular menu items, update all visual indicators
        restoreSelectedItems();
        populateSavedOrdersCarousel();
    }

    // Reset the flags so we don't unselect on modal close
    wasAlreadySelected = false;
    cancelButtonClicked = false;

    const modalInstance = bootstrap.Modal.getInstance(itemModalEl);
    if (modalInstance) modalInstance.hide();
});

// --- Modal Cancel button (in modal footer) ---
const modalCancelBtn = itemModalEl.querySelector('.modal-footer .btn-cancel');
if (modalCancelBtn) {
    modalCancelBtn.addEventListener('click', () => {
        cancelButtonClicked = true;
        // Modal will close via data-bs-dismiss, and hidden.bs.modal will handle unselecting
    });
}

// --- Main page Cancel button ---
const cancelBtn = document.getElementById('cancelBtn');
if (cancelBtn) {
    cancelBtn.addEventListener('click', () => {
        const items = getSelectedItems();
        
        // If there are selected items, show confirmation modal
        if (items.length > 0) {
            const cancelOrderModal = new bootstrap.Modal(document.getElementById('cancelOrderModal'));
            cancelOrderModal.show();
        } else {
            // No items selected, just go back to home
            window.location.href = '/Home/Index';
        }
    });
}

// --- Confirm Cancel Order button ---
const confirmCancelBtn = document.getElementById('confirmCancelOrder');
if (confirmCancelBtn) {
    confirmCancelBtn.addEventListener('click', () => {
        // Clear all selected items and special requests
        localStorage.removeItem('selectedItems');
        localStorage.removeItem('specialRequests');
        
        // Close modal and redirect
        const cancelOrderModal = bootstrap.Modal.getInstance(document.getElementById('cancelOrderModal'));
        if (cancelOrderModal) {
            cancelOrderModal.hide();
        }
        
        // Redirect to home
        window.location.href = '/Home/Index';
    });
}

// --- Menu switching functionality ---
document.querySelectorAll('.menu-option').forEach(option => {
    option.addEventListener('click', (e) => {
        e.preventDefault();
        const selectedMenu = option.getAttribute('data-menu');
        
        // Update current displayed menu
        currentDisplayedMenu = selectedMenu;
        
        // Update header
        updateHeaderDisplay(selectedMenu, selectedMenu === servicePeriod);
        
        // Update menu items availability
        updateMenuItemsAvailability(selectedMenu);
        
        // Update order button state
        if (selectedMenu !== servicePeriod && !isViewingMode && !isDebugMode) {
            // Disable order button if viewing non-active menu
            orderBtn.disabled = true;
            orderBtn.style.opacity = '0.5';
            orderBtn.style.cursor = 'not-allowed';
            orderBtn.title = 'Orders can only be placed during active service hours';
        } else if (!isViewingMode || isDebugMode) {
            // Enable order button if viewing active menu or in debug mode
            orderBtn.disabled = false;
            orderBtn.style.opacity = '1';
            orderBtn.style.cursor = 'pointer';
            orderBtn.title = '';
        }
    });
});

// --- Order button ---
if (orderBtn) {
    orderBtn.addEventListener('click', () => {
        // Check if button is disabled
        if (orderBtn.disabled) {
            return;
        }
        
        const items = getSelectedItems();
        if (!items.length) {
            const noItemsModal = new bootstrap.Modal(document.getElementById('noItemsModal'));
            noItemsModal.show();
            return;
        }
        window.location.href = '/Home/Checkout';
    });
}

// --- Special requests sync ---
if (specialInput && hiddenSpecial) {
    specialInput.addEventListener('input', () => {
        hiddenSpecial.value = specialInput.value;
        localStorage.setItem('specialRequests', specialInput.value);
    });
}

// --- Microphone button ---
let currentRecognition = null;

if (micButton && specialInput) {
    micButton.addEventListener('click', () => {
        // If already listening, stop it
        if (currentRecognition && micButton.classList.contains('listening')) {
            currentRecognition.stop();
            currentRecognition = null;
            micButton.classList.remove('listening');
            return;
        }
        
        const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
        if (!SpeechRecognition) {
            const old = specialInput.placeholder;
            specialInput.placeholder = 'Speech not available — type instead';
            setTimeout(() => specialInput.placeholder = old, 1500);
            return;
        }

        const recog = new SpeechRecognition();
        recog.lang = 'en-US';
        recog.interimResults = false;
        recog.maxAlternatives = 1;
        
        currentRecognition = recog;
        
        recog.start();
        micButton.classList.add('listening');

        recog.onresult = (e) => {
            const spoken = e.results[0][0].transcript;
            specialInput.value = specialInput.value ? specialInput.value + ' ' + spoken : spoken;
            hiddenSpecial.value = specialInput.value;
            localStorage.setItem('specialRequests', specialInput.value);
        };
        
        recog.onend = () => {
            micButton.classList.remove('listening');
            currentRecognition = null;
        };
        
        recog.onerror = () => {
            micButton.classList.remove('listening');
            currentRecognition = null;
        };
    });
}

// --- Image error handling ---
function setupImageErrorHandling() {
    document.querySelectorAll('.menu-item').forEach(img => {
        img.addEventListener('error', function() {
            this.classList.add('image-error');
            // Hide the broken image and show fallback styling
            this.style.content = '';
            this.src = 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMSIgaGVpZ2h0PSIxIiB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciPjxyZWN0IHdpZHRoPSIxMDAlIiBoZWlnaHQ9IjEwMCUiIGZpbGw9InRyYW5zcGFyZW50Ii8+PC9zdmc+';
        });
        
        img.addEventListener('load', function() {
            // Remove error class if image loads successfully
            this.classList.remove('image-error');
        });
    });
}

// --- Function to remove all instances of an item ---
function removeAllInstancesOfItem(itemName, itemType) {
    const items = getSelectedItems();
    const appliedSavedOrders = getAppliedSavedOrders();
    
    // Remove all instances of this item
    const filteredItems = items.filter(item => 
        !(item.name === itemName && item.type === itemType)
    );
    
    // Remove any applied saved orders for this item
    const filteredAppliedOrders = appliedSavedOrders.filter(applied => 
        !(applied.itemName === itemName && applied.itemType === itemType)
    );
    
    saveSelectedItems(filteredItems);
    saveAppliedSavedOrders(filteredAppliedOrders);
    
    // Refresh both regular menu items and saved orders carousel to update indicators
    restoreSelectedItems();
    populateSavedOrdersCarousel();
}

// --- Function to remove only manual selections of an item ---
function removeManualSelectionsOfItem(itemName, itemType) {
    const items = getSelectedItems();
    
    // Remove only manual selections (keep items from saved orders)
    const filteredItems = items.filter(item => 
        !(item.name === itemName && item.type === itemType && !item.fromSavedOrder)
    );
    
    saveSelectedItems(filteredItems);
    
    // Refresh visual indicators
    restoreSelectedItems();
    populateSavedOrdersCarousel();
}

// --- Restore selected items on page load ---
function restoreSelectedItems() {
    const items = getSelectedItems();
    
    // First, clear all existing indicators on regular menu items (not saved orders)
    document.querySelectorAll('.menu-item:not(.saved-order-item)').forEach(img => {
        const wrapper = img.closest('.carousel-item');
        if (wrapper) {
            wrapper.classList.remove('selected');
            const existingIndicator = wrapper.querySelector('.selected-indicator:not(.saved-order-indicator)');
            if (existingIndicator) {
                existingIndicator.remove();
            }
        }
    });
    
    if (!items.length) return;

    // Find all menu item images and match them with selected items
    // Only show indicators for manual selections (not from saved orders)
    document.querySelectorAll('.menu-item:not(.saved-order-item)').forEach(img => {
        const itemName = img.getAttribute('data-item') || img.alt;
        const itemType = img.getAttribute('data-type') || 'drink';
        
        // Check if this item has manual selections (not from saved orders)
        const manualSelections = items.filter(item => 
            item.name === itemName && 
            item.type === itemType && 
            !item.fromSavedOrder
        );
        
        if (manualSelections.length > 0) {
            const wrapper = img.closest('.carousel-item');
            if (wrapper && !wrapper.querySelector('.selected-indicator:not(.saved-order-indicator)')) {
                wrapper.classList.add('selected');
                
                // Get total quantity for manual selections only
                const totalQuantity = manualSelections.reduce((sum, item) => sum + (item.quantity || 1), 0);
                
                const indicator = document.createElement('div');
                indicator.className = 'selected-indicator manual-selection-indicator';
                if (totalQuantity > 1) {
                    indicator.innerHTML = `<i class="bi bi-check-circle-fill"></i><span class="quantity-badge">${totalQuantity}</span>`;
                } else {
                    indicator.innerHTML = '<i class="bi bi-check-circle-fill"></i>';
                }
                
                // Add hover effect to turn checkmark into red X
                indicator.addEventListener('mouseenter', function() {
                    this.classList.add('remove-mode');
                    const icon = this.querySelector('i');
                    if (icon) {
                        icon.className = 'bi bi-x-circle-fill';
                    }
                });
                
                indicator.addEventListener('mouseleave', function() {
                    this.classList.remove('remove-mode');
                    const icon = this.querySelector('i');
                    if (icon) {
                        icon.className = 'bi bi-check-circle-fill';
                    }
                });
                
                // Add click handler to remove only manual selections of this item
                indicator.addEventListener('click', function(e) {
                    e.stopPropagation(); // Prevent modal from opening
                    e.preventDefault();
                    
                    // Remove only manual selections of this item (keep saved order items)
                    removeManualSelectionsOfItem(itemName, itemType);
                    
                    // Remove visual indicator
                    wrapper.classList.remove('selected');
                    this.remove();
                });
                
                // Make indicator clickable
                indicator.style.pointerEvents = 'auto';
                indicator.style.cursor = 'pointer';
                
                wrapper.appendChild(indicator);
            }
        }
    });
}

// --- Populate Recent Orders Carousel ---
function populateRecentOrdersCarousel() {
    const carousel = document.getElementById('carouselRecent');
    if (!carousel) return;
    
    const carouselInner = carousel.querySelector('.carousel-inner');
    if (!carouselInner) return;
    
    const recentOrders = getRecentOrders();
    
    if (recentOrders.length === 0) {
        carouselInner.innerHTML = `
            <div class="carousel-item active text-center">
                <div class="empty-carousel-message">
                    <i class="bi bi-clock-history mb-2"></i>
                    <p>You haven't made any orders yet</p>
                </div>
            </div>
        `;
        const prevBtn = carousel.querySelector('.carousel-control-prev');
        const nextBtn = carousel.querySelector('.carousel-control-next');
        if (prevBtn) prevBtn.style.display = 'none';
        if (nextBtn) nextBtn.style.display = 'none';
        return;
    }
    
    const prevBtn = carousel.querySelector('.carousel-control-prev');
    const nextBtn = carousel.querySelector('.carousel-control-next');
    if (prevBtn) prevBtn.style.display = 'flex';
    if (nextBtn) nextBtn.style.display = 'flex';
    
    // Create carousel items from recent orders
    const carouselItems = recentOrders.slice(0, 10).map((order, index) => {
        const firstItem = order.items && order.items.length > 0 ? order.items[0] : null;
        if (!firstItem) return '';
        
        const totalItemCount = order.items.reduce((sum, item) => sum + (item.quantity || 1), 0);
        const defaultImage = 'https://images.unsplash.com/photo-1572442388796-11668a67e53d?ixlib=rb-4.1.0&auto=format&fit=crop&q=80&w=1170';
        
        let caption = firstItem.name;
        if (firstItem.quantity && firstItem.quantity > 1) {
            caption += ` x${firstItem.quantity}`;
        }
        if (order.items.length > 1) {
            const remainingCount = totalItemCount - (firstItem.quantity || 1);
            caption += ` +${remainingCount} more`;
        }
        
        return `
            <div class="carousel-item ${index === 0 ? 'active' : ''} text-center">
                <img src="${defaultImage}"
                     class="d-block mx-auto rounded carousel-img recent-order-item"
                     alt="${firstItem.name}"
                     data-item="${firstItem.name}"
                     data-type="${firstItem.type || 'drink'}"
                     data-order-data='${JSON.stringify(order).replace(/'/g, '&apos;')}'
                     style="cursor: pointer;">
                <div class="carousel-caption-text">${caption}</div>
            </div>
        `;
    }).filter(item => item !== '');
    
    carouselInner.innerHTML = carouselItems.join('');
    
    // Add click handlers for recent order items
    carouselInner.querySelectorAll('.recent-order-item').forEach(img => {
        img.addEventListener('click', function(e) {
            e.stopPropagation();
            const orderData = JSON.parse(this.getAttribute('data-order-data'));
            const itemName = this.getAttribute('data-item');
            const itemType = this.getAttribute('data-type');
            loadRecentOrderFromCarousel(orderData, itemName, itemType);
        });
    });
}

// --- Populate Saved Orders Carousel ---
function populateSavedOrdersCarousel() {
    const carousel = document.getElementById('carouselSaved');
    if (!carousel) return;
    
    const carouselInner = carousel.querySelector('.carousel-inner');
    if (!carouselInner) return;
    
    const savedOrders = getSavedOrders();
    const appliedSavedOrders = getAppliedSavedOrders();
    
    if (savedOrders.length === 0) {
        carouselInner.innerHTML = `
            <div class="carousel-item active text-center">
                <div class="empty-carousel-message">
                    <i class="bi bi-bookmark mb-2"></i>
                    <p>You haven't saved any orders yet</p>
                </div>
            </div>
        `;
        // Hide carousel controls
        const prevBtn = carousel.querySelector('.carousel-control-prev');
        const nextBtn = carousel.querySelector('.carousel-control-next');
        if (prevBtn) prevBtn.style.display = 'none';
        if (nextBtn) nextBtn.style.display = 'none';
        return;
    }
    
    // Show carousel controls
    const prevBtn = carousel.querySelector('.carousel-control-prev');
    const nextBtn = carousel.querySelector('.carousel-control-next');
    if (prevBtn) prevBtn.style.display = 'flex';
    if (nextBtn) nextBtn.style.display = 'flex';
    
    // Create carousel items from saved orders
    const carouselItems = savedOrders.slice(0, 10).map((order, index) => {
        // Get the actual image for this item from the menu
        let itemImage = 'https://images.unsplash.com/photo-1555507036-ab1f4038808a?ixlib=rb-4.1.0&auto=format&fit=crop&q=80&w=1170'; // fallback
        
        // Find the actual menu item to get its image
        const menuItem = document.querySelector(`[data-item="${order.itemName}"][data-type="${order.itemType}"]:not(.saved-order-item)`);
        if (menuItem && menuItem.src) {
            itemImage = menuItem.src;
        }
        
        const tabCount = order.tabs ? order.tabs.length : 1;
        const caption = tabCount > 1 ? `${order.name} (${tabCount}x)` : order.name;
        
        // Check if this saved order is currently applied
        const isApplied = appliedSavedOrders.some(appliedOrder => appliedOrder.id === order.id);
        const appliedOrder = appliedSavedOrders.find(appliedOrder => appliedOrder.id === order.id);
        const totalQuantity = appliedOrder ? appliedOrder.totalQuantity : 0;
        
        const selectedClass = isApplied ? ' selected saved-order-applied' : '';
        const selectedIndicator = isApplied ? 
            (totalQuantity > 1 ? 
                `<div class="selected-indicator saved-order-indicator"><i class="bi bi-bookmark-check-fill"></i><span class="quantity-badge">${totalQuantity}</span></div>` :
                `<div class="selected-indicator saved-order-indicator"><i class="bi bi-bookmark-check-fill"></i></div>`
            ) : '';
        
        return `
            <div class="carousel-item ${index === 0 ? 'active' : ''} text-center saved-order-wrapper${selectedClass}">
                <div class="saved-order-edit-btn" data-order-id="${order.id}" title="Edit saved order">
                    <i class="bi bi-gear-fill"></i>
                </div>
                <div class="saved-order-delete-btn" data-order-id="${order.id}" title="Delete saved order">
                    <i class="bi bi-trash-fill"></i>
                </div>
                <img src="${itemImage}"
                     class="d-block mx-auto rounded carousel-img saved-order-item"
                     alt="${order.name}"
                     data-item="${order.itemName}"
                     data-type="${order.itemType}"
                     data-order-data='${JSON.stringify(order).replace(/'/g, '&apos;')}'
                     style="cursor: pointer;">
                <div class="carousel-caption-text">${caption}</div>
                ${selectedIndicator}
            </div>
        `;
    });
    
    carouselInner.innerHTML = carouselItems.join('');
    
    // Add click handlers for saved order items (apply directly)
    carouselInner.querySelectorAll('.saved-order-item').forEach(img => {
        img.addEventListener('click', function(e) {
            e.stopPropagation();
            const orderData = JSON.parse(this.getAttribute('data-order-data'));
            loadSavedOrderFromCarousel(orderData, false);
        });
    });
    
    // Add click handlers for edit buttons
    carouselInner.querySelectorAll('.saved-order-edit-btn').forEach(btn => {
        btn.addEventListener('click', function(e) {
            e.stopPropagation();
            const orderId = parseInt(this.getAttribute('data-order-id'));
            const savedOrders = getSavedOrders();
            const orderData = savedOrders.find(o => o.id === orderId);
            if (orderData) {
                loadSavedOrderFromCarousel(orderData, true);
            }
        });
    });
    
    // Add click handlers for delete buttons
    carouselInner.querySelectorAll('.saved-order-delete-btn').forEach(btn => {
        btn.addEventListener('click', function(e) {
            e.stopPropagation();
            const orderId = parseInt(this.getAttribute('data-order-id'));
            if (confirm('Are you sure you want to delete this saved order?')) {
                deleteSavedOrder(orderId);
            }
        });
    });
    
    // Add click handlers for selected indicators on saved orders
    carouselInner.querySelectorAll('.saved-order-indicator').forEach(indicator => {
        const wrapper = indicator.closest('.saved-order-wrapper');
        const img = wrapper.querySelector('.saved-order-item');
        const orderData = JSON.parse(img.getAttribute('data-order-data'));
        
        // Add hover effect to turn bookmark into red X
        indicator.addEventListener('mouseenter', function() {
            this.classList.add('remove-mode');
            const icon = this.querySelector('i');
            if (icon) {
                icon.className = 'bi bi-x-circle-fill';
            }
        });
        
        indicator.addEventListener('mouseleave', function() {
            this.classList.remove('remove-mode');
            const icon = this.querySelector('i');
            if (icon) {
                icon.className = 'bi bi-bookmark-check-fill';
            }
        });
        
        // Add click handler to remove this saved order from applied orders
        indicator.addEventListener('click', function(e) {
            e.stopPropagation();
            e.preventDefault();
            
            // Remove this saved order from applied orders
            removeAppliedSavedOrder(orderData.id);
            
            // Refresh the saved orders carousel to update indicators
            populateSavedOrdersCarousel();
        });
        
        // Make indicator clickable
        indicator.style.pointerEvents = 'auto';
        indicator.style.cursor = 'pointer';
    });
}

// --- Load Recent Order ---
function loadRecentOrder(orderData) {
    if (!orderData || !orderData.items || orderData.items.length === 0) return;
    
    // Clear current selection
    localStorage.removeItem('selectedItems');
    
    // Add all items from the recent order to current selection
    const itemsToAdd = [];
    orderData.items.forEach(item => {
        itemsToAdd.push({
            name: item.name,
            type: item.type || 'drink',
            modifiers: item.modifiers || [],
            specialRequests: item.specialRequests || '',
            quantity: item.quantity || 1
        });
    });
    
    saveSelectedItems(itemsToAdd);
    
    // Restore visual indicators
    restoreSelectedItems();
    
    alert(`Recent order loaded! ${itemsToAdd.length} item(s) added to your cart.`);
}

// --- Apply Saved Order Directly ---
function applySavedOrderDirectly(savedOrder) {
    if (!savedOrder || !savedOrder.tabs) return;
    
    const selectedItems = getSelectedItems();
    const appliedSavedOrders = getAppliedSavedOrders();
    
    // Add all tab instances from saved order to selected items
    savedOrder.tabs.forEach(tabData => {
        const itemObj = {
            name: savedOrder.itemName,
            type: savedOrder.itemType,
            modifiers: tabData.modifiers || [],
            specialRequests: tabData.specialRequests || '',
            quantity: 1,
            fromSavedOrder: savedOrder.id // Mark as from saved order
        };
        
        // Check if an identical item already exists
        const identicalIndex = selectedItems.findIndex(item => areItemsIdentical(item, itemObj));
        
        if (identicalIndex !== -1) {
            // Item is identical - merge by increasing quantity
            selectedItems[identicalIndex].quantity = (selectedItems[identicalIndex].quantity || 1) + 1;
        } else {
            // Item is different - add as new entry
            selectedItems.push(itemObj);
        }
    });
    
    // Track this saved order as applied
    const existingAppliedIndex = appliedSavedOrders.findIndex(applied => applied.id === savedOrder.id);
    const totalQuantity = savedOrder.tabs.length;
    
    if (existingAppliedIndex !== -1) {
        appliedSavedOrders[existingAppliedIndex].totalQuantity += totalQuantity;
    } else {
        appliedSavedOrders.push({
            id: savedOrder.id,
            name: savedOrder.name,
            itemName: savedOrder.itemName,
            itemType: savedOrder.itemType,
            totalQuantity: totalQuantity,
            appliedAt: Date.now()
        });
    }
    
    saveSelectedItems(selectedItems);
    saveAppliedSavedOrders(appliedSavedOrders);
    
    // Update visual indicators
    restoreSelectedItems();
    populateSavedOrdersCarousel();
    
    showNotification(`"${savedOrder.name}" has been applied to your order!`, 'success');
}

// --- Apply Saved Order Directly (Silent - no notification) ---
function applySavedOrderDirectlySilent(savedOrder) {
    if (!savedOrder || !savedOrder.tabs) return;
    
    const selectedItems = getSelectedItems();
    const appliedSavedOrders = getAppliedSavedOrders();
    
    // Add all tab instances from saved order to selected items
    savedOrder.tabs.forEach(tabData => {
        const itemObj = {
            name: savedOrder.itemName,
            type: savedOrder.itemType,
            modifiers: tabData.modifiers || [],
            specialRequests: tabData.specialRequests || '',
            quantity: 1,
            fromSavedOrder: savedOrder.id // Mark as from saved order
        };
        
        // Check if an identical item already exists
        const identicalIndex = selectedItems.findIndex(item => areItemsIdentical(item, itemObj));
        
        if (identicalIndex !== -1) {
            // Item is identical - merge by increasing quantity
            selectedItems[identicalIndex].quantity = (selectedItems[identicalIndex].quantity || 1) + 1;
        } else {
            // Item is different - add as new entry
            selectedItems.push(itemObj);
        }
    });
    
    // Track this saved order as applied
    const existingAppliedIndex = appliedSavedOrders.findIndex(applied => applied.id === savedOrder.id);
    const totalQuantity = savedOrder.tabs.length;
    
    if (existingAppliedIndex !== -1) {
        appliedSavedOrders[existingAppliedIndex].totalQuantity += totalQuantity;
    } else {
        appliedSavedOrders.push({
            id: savedOrder.id,
            name: savedOrder.name,
            itemName: savedOrder.itemName,
            itemType: savedOrder.itemType,
            totalQuantity: totalQuantity,
            appliedAt: Date.now()
        });
    }
    
    saveSelectedItems(selectedItems);
    saveAppliedSavedOrders(appliedSavedOrders);
    
    // Update visual indicators
    restoreSelectedItems();
    populateSavedOrdersCarousel();
    
    // No notification - user already confirmed via dialog
}

// --- Remove Applied Saved Order ---
function removeAppliedSavedOrder(savedOrderId) {
    const selectedItems = getSelectedItems();
    const appliedSavedOrders = getAppliedSavedOrders();
    
    // Remove items that came from this saved order
    const filteredItems = selectedItems.filter(item => item.fromSavedOrder !== savedOrderId);
    
    // Remove from applied saved orders
    const filteredAppliedOrders = appliedSavedOrders.filter(applied => applied.id !== savedOrderId);
    
    saveSelectedItems(filteredItems);
    saveAppliedSavedOrders(filteredAppliedOrders);
    
    // Update visual indicators
    restoreSelectedItems();
    populateSavedOrdersCarousel();
    
    const appliedOrder = appliedSavedOrders.find(applied => applied.id === savedOrderId);
    if (appliedOrder) {
        showNotification(`"${appliedOrder.name}" has been removed from your order.`, 'success');
    }
}

// --- Load Saved Order from Carousel ---
function loadSavedOrderFromCarousel(savedOrder, isEditMode = false) {
    if (!savedOrder) return;
    
    // Check if item is available (not disabled) unless in debug mode
    if (!isDebugMode) {
        const isAvailable = (servicePeriod === 'breakfast' && breakfastItems.includes(savedOrder.itemName)) ||
                           (servicePeriod === 'lunch' && lunchItems.includes(savedOrder.itemName));
        
        if (!isAvailable && !isViewingMode) {
            showNotification('This item is not available during the current service period.', 'error');
            return;
        }
    }
    
    // If edit mode, open modal for editing
    if (isEditMode) {
        window.pendingSavedOrder = savedOrder;
        const modal = new bootstrap.Modal(itemModalEl);
        modal.show();
        return;
    }
    
    // Check if this saved order is already applied
    const appliedSavedOrders = getAppliedSavedOrders();
    const isAlreadyApplied = appliedSavedOrders.some(applied => applied.id === savedOrder.id);
    
    if (isAlreadyApplied) {
        // Show confirmation dialog for already applied orders - only once
        if (confirm(`"${savedOrder.name}" has already been added to your order.\n\nWould you like to add it again?`)) {
            // Apply directly without showing notification since user already confirmed
            applySavedOrderDirectlySilent(savedOrder);
        }
        return;
    }
    
    // Otherwise, apply directly with notification
    applySavedOrderDirectly(savedOrder);
}

// --- Load Recent Order from Carousel ---
function loadRecentOrderFromCarousel(recentOrder, itemName, itemType) {
    if (!recentOrder) return;
    
    // Check if item is available (not disabled) unless in debug mode
    if (!isDebugMode) {
        const isAvailable = (servicePeriod === 'breakfast' && breakfastItems.includes(itemName)) ||
                           (servicePeriod === 'lunch' && lunchItems.includes(itemName));
        
        if (!isAvailable && !isViewingMode) {
            showNotification('This item is not available during the current service period.', 'error');
            return;
        }
    }
    
    // Set current item info for the modal
    currentItemName = itemName;
    currentItemType = itemType;
    
    // Store the recent order data for loading
    window.pendingRecentOrder = recentOrder;
    
    // Open the modal directly
    const modal = new bootstrap.Modal(itemModalEl);
    modal.show();
}

// --- Delete Saved Order ---
function deleteSavedOrder(orderId) {
    const savedOrders = getSavedOrders();
    const orderIndex = savedOrders.findIndex(o => o.id === orderId);
    
    if (orderIndex !== -1) {
        const orderName = savedOrders[orderIndex].name;
        savedOrders.splice(orderIndex, 1);
        saveSavedOrders(savedOrders);
        populateSavedOrdersCarousel();
        showNotification(`"${orderName}" has been deleted.`, 'success');
    }
}

// --- Initialize carousels ---
document.querySelectorAll('.carousel').forEach(car => {
    new bootstrap.Carousel(car, { interval: false, ride: false, wrap: true, touch: true });
});

// Populate carousels with saved and recent orders
populateRecentOrdersCarousel();
populateSavedOrdersCarousel();

// Setup image error handling
setupImageErrorHandling();

// Restore selected items when page loads
restoreSelectedItems();

// --- Handle navigation state persistence ---
// Check if we're returning from checkout
const checkoutUrlParams = new URLSearchParams(window.location.search);
const fromCheckout = checkoutUrlParams.get('from') === 'checkout';

if (fromCheckout) {
    // Clean up URL without refreshing page
    const newUrl = window.location.pathname;
    window.history.replaceState({}, document.title, newUrl);
}

// --- Clear applied saved orders when canceling ---
function clearAppliedSavedOrders() {
    localStorage.removeItem('appliedSavedOrders');
    populateSavedOrdersCarousel();
}

// Update the cancel button to also clear applied saved orders
const confirmCancelOrderBtn = document.getElementById('confirmCancelOrder');
if (confirmCancelOrderBtn) {
    const originalHandler = confirmCancelOrderBtn.onclick;
    confirmCancelOrderBtn.onclick = function() {
        // Clear applied saved orders as well
        clearAppliedSavedOrders();
        if (originalHandler) {
            originalHandler.call(this);
        }
    };
}

});
