document.addEventListener('DOMContentLoaded', () => {
    // --- Time-based Menu Management ---
    const urlParams = new URLSearchParams(window.location.search);
    const viewMode = urlParams.get('view'); // 'breakfast' or 'lunch' for viewing mode
    const isViewingMode = viewMode !== null;
    
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
    
    // Redirect to closed page if outside service hours and not in viewing mode
    if (servicePeriod === 'closed' && !isViewingMode) {
        window.location.href = '/Home/Closed';
        return;
    }
    
    // Update header based on service period
    const servingStatus = document.getElementById('servingStatus');
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
    
    // Function to update menu items availability
    function updateMenuItemsAvailability(currentPeriod) {
        document.querySelectorAll('.menu-item').forEach(img => {
            const itemName = img.getAttribute('data-item') || img.alt;
            const wrapper = img.closest('.carousel-item');
            
            if (!wrapper) return;
            
            let isAvailable = false;
            let availabilityMessage = '';
            
            if (currentPeriod === 'breakfast') {
                isAvailable = breakfastItems.includes(itemName);
                availabilityMessage = 'Available during Lunch hours';
            } else if (currentPeriod === 'lunch') {
                isAvailable = lunchItems.includes(itemName);
                availabilityMessage = 'Available during Breakfast hours';
            }
            
            // Remove previous disabled state
            wrapper.classList.remove('disabled');
            wrapper.removeAttribute('data-availability-message');
            img.setAttribute('data-bs-toggle', 'modal');
            img.setAttribute('data-bs-target', '#itemModal');
            img.style.cursor = 'pointer';
            
            if (!isAvailable || isViewingMode) {
                wrapper.classList.add('disabled');
                wrapper.setAttribute('data-availability-message', availabilityMessage);
                // Remove modal trigger attributes
                img.removeAttribute('data-bs-toggle');
                img.removeAttribute('data-bs-target');
                img.style.cursor = 'not-allowed';
            }
        });
    }
    
    // Initial update
    updateMenuItemsAvailability(servicePeriod);
    
    // --- Helpers ---
    function getSelectedItems() { return JSON.parse(localStorage.getItem('selectedItems') || '[]'); }
    function saveSelectedItems(items) { localStorage.setItem('selectedItems', JSON.stringify(items)); }

 
let currentItemName = null;
let currentItemType = null;
let currentTriggerImg = null;
let wasAlreadySelected = false; // Track if item was already selected when modal opened
let currentSelectedItemData = null; // Store the current item's saved data
let cancelButtonClicked = false; // Track if Cancel button was clicked
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
    cancelButtonClicked = false; // Reset cancel flag
    
    if (trigger) {
        currentItemName = trigger.getAttribute('data-item') || trigger.alt || 'Item';
        currentItemType = trigger.getAttribute('data-type') || 'drink'; // default to drink if not specified
        currentTriggerImg = trigger;
        
        // Check if this item is already selected (any variation)
        wasAlreadySelected = hasItemWithSameNameAndType(currentItemName, currentItemType);
        
        // Update modal title
        const modalTitle = document.getElementById('itemModalLabel');
        if (wasAlreadySelected) {
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

    // Hide carousel arrows
    [...document.getElementsByClassName('carousel-control-prev')].forEach(btn => btn.style.display = 'none');
    [...document.getElementsByClassName('carousel-control-next')].forEach(btn => btn.style.display = 'none');

    // Initialize tabs - restore existing instances or create new
    const existingItems = getSelectedItems().filter(item => 
        item.name === currentItemName && item.type === currentItemType
    );
    
    if (existingItems.length > 0) {
        // Restore existing instances as tabs
        // Expand items with quantity > 1 into multiple tabs
        tabInstances = [];
        existingItems.forEach((item) => {
            const quantity = item.quantity || 1;
            // Create a tab for each instance (if quantity > 1, create multiple tabs with same properties)
            for (let i = 0; i < quantity; i++) {
                tabInstances.push({
                    id: `tab-${++tabIdCounter}`,
                    modifiers: Array.isArray(item.modifiers) ? [...item.modifiers] : [], // Deep copy
                    specialRequests: item.specialRequests || ''
                });
            }
        });
        
        activeTabId = tabInstances[0].id;
        renderTabs();
        
        // Load the first tab's state immediately
        // Use setTimeout to ensure DOM is ready and item type is set
        setTimeout(() => {
            if (tabInstances.length > 0 && activeTabId) {
                // Don't save state on initial load - just load it
                const firstTab = tabInstances.find(t => t.id === activeTabId);
                if (firstTab) {
                    activeTabId = firstTab.id;
                    // Update tab UI
                    if (instanceTabsContainer) {
                        instanceTabsContainer.querySelectorAll('.instance-tab').forEach(tab => {
                            tab.classList.toggle('active', tab.dataset.tabId === activeTabId);
                        });
                    }
                    // Load the state
                    loadTabState(firstTab);
                }
            }
            adjustTabWidths();
        }, 50);
    } else {
        // Initialize with new tab
        initializeTabs();
        // Adjust tab widths after a short delay to ensure container is rendered
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
});

// --- Add Tab Button ---
if (addTabBtn) {
    addTabBtn.addEventListener('click', addNewTab);
}

// --- Apply modifiers ---
applyBtn.addEventListener('click', () => {
    if (!currentItemName) return alert("Please select an item before applying modifiers.");

    // Save current tab state
    saveCurrentTabState();

    const items = getSelectedItems();
    
    // Remove all existing instances of this item
    const filteredItems = items.filter(item => 
        !(item.name === currentItemName && item.type === currentItemType)
    );
    
    // Add all tab instances as separate items (each with quantity 1)
    tabInstances.forEach(instance => {
        const itemObj = {
            name: currentItemName,
            type: currentItemType,
            modifiers: instance.modifiers || [],
            specialRequests: instance.specialRequests || '',
            quantity: 1 // Each tab is one instance
        };
        
        // Check if an identical item already exists in the filtered list
        const identicalIndex = filteredItems.findIndex(item => areItemsIdentical(item, itemObj));
        
        if (identicalIndex !== -1) {
            // Item is identical - merge by increasing quantity
            filteredItems[identicalIndex].quantity = (filteredItems[identicalIndex].quantity || 1) + 1;
        } else {
            // Item is different - add as new entry
            filteredItems.push(itemObj);
        }
    });
    
    saveSelectedItems(filteredItems);

    // Update visual indicator with quantity badge
    if (currentTriggerImg) {
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
                e.stopPropagation(); // Prevent modal from opening
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
        if (selectedMenu !== servicePeriod && !isViewingMode) {
            // Disable order button if viewing non-active menu
            orderBtn.disabled = true;
            orderBtn.style.opacity = '0.5';
            orderBtn.style.cursor = 'not-allowed';
            orderBtn.title = 'Orders can only be placed during active service hours';
        } else if (!isViewingMode) {
            // Enable order button if viewing active menu
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
if (micButton && specialInput) {
    micButton.addEventListener('click', () => {
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
        recog.start();
        micButton.classList.add('active');

        recog.onresult = (e) => {
            const spoken = e.results[0][0].transcript;
            specialInput.value = specialInput.value ? specialInput.value + ' ' + spoken : spoken;
            hiddenSpecial.value = specialInput.value;
            localStorage.setItem('specialRequests', specialInput.value);
        };
        recog.onend = () => micButton.classList.remove('active');
        recog.onerror = () => micButton.classList.remove('active');
    });
}

// --- Function to remove all instances of an item ---
function removeAllInstancesOfItem(itemName, itemType) {
    const items = getSelectedItems();
    const filteredItems = items.filter(item => 
        !(item.name === itemName && item.type === itemType)
    );
    saveSelectedItems(filteredItems);
}

// --- Restore selected items on page load ---
function restoreSelectedItems() {
    const items = getSelectedItems();
    if (!items.length) return;

    // Find all menu item images and match them with selected items
    document.querySelectorAll('.menu-item').forEach(img => {
        const itemName = img.getAttribute('data-item') || img.alt;
        const itemType = img.getAttribute('data-type') || 'drink';
        
        // Check if this item is in the selected items list
        const isSelected = items.some(item => 
            item.name === itemName && item.type === itemType
        );
        
        if (isSelected) {
            const wrapper = img.closest('.carousel-item');
            if (wrapper && !wrapper.querySelector('.selected-indicator')) {
                wrapper.classList.add('selected');
                
                // Get total quantity for this item
                const totalQuantity = getItemTotalQuantity(itemName, itemType);
                
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
                    e.stopPropagation(); // Prevent modal from opening
                    e.preventDefault();
                    
                    // Remove all instances of this item
                    removeAllInstancesOfItem(itemName, itemType);
                    
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

// --- Initialize carousels ---
document.querySelectorAll('.carousel').forEach(car => {
    new bootstrap.Carousel(car, { interval: false, ride: false, wrap: true, touch: true });
});

// Restore selected items when page loads
restoreSelectedItems();

});
