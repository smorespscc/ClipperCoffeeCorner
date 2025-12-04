/**
 * API Client for backend communication
 * Replaces direct localStorage access with server API calls
 */
class ApiClient {
    constructor() {
        this.baseUrl = '/api';
    }

    // ==================== CART OPERATIONS ====================
    
    async addToCart(orderItem) {
        return await this.post('/order/cart/add', orderItem);
    }

    async removeFromCart(index) {
        return await this.delete(`/order/cart/remove/${index}`);
    }

    async removeAllInstances(itemName, itemType) {
        return await this.delete(`/order/cart/remove-all?itemName=${encodeURIComponent(itemName)}&itemType=${encodeURIComponent(itemType)}`);
    }

    async getCart() {
        return await this.get('/order/cart');
    }

    async clearCart() {
        return await this.delete('/order/cart/clear');
    }

    // ==================== ORDER OPERATIONS ====================
    
    async createOrder() {
        return await this.post('/order/create', {});
    }

    async processPayment(paymentMethod, paymentDetails) {
        return await this.post('/order/payment', { paymentMethod, paymentDetails });
    }

    async getRecentOrders() {
        return await this.get('/order/recent');
    }

    // ==================== SAVED ORDERS ====================
    
    async getSavedOrders() {
        return await this.get('/order/saved');
    }

    async saveCurrentCart(orderName) {
        return await this.post('/order/saved', { orderName });
    }

    async applySavedOrder(savedOrderId) {
        return await this.post(`/order/saved/${savedOrderId}/apply`, {});
    }

    async deleteSavedOrder(savedOrderId) {
        return await this.delete(`/order/saved/${savedOrderId}`);
    }

    // ==================== MENU OPERATIONS ====================
    
    async getMenuItems() {
        return await this.get('/menu/items');
    }

    async getAvailableItems() {
        return await this.get('/menu/available');
    }

    async getTrendingItems() {
        return await this.get('/menu/trending');
    }

    async getSpecialItems() {
        return await this.get('/menu/specials');
    }

    async getServicePeriod() {
        return await this.get('/menu/service-period');
    }

    async getModifiers(type) {
        return await this.get(`/menu/modifiers/${type}`);
    }

    // ==================== CUSTOMER OPERATIONS ====================
    
    async login(email, password, isStaff, staffCode) {
        return await this.post('/customer/login', { email, password, isStaff, staffCode });
    }

    async logout() {
        return await this.post('/customer/logout', {});
    }

    async register(username, email, phone, password, emailNotifications, textNotifications) {
        return await this.post('/customer/register', {
            username, email, phone, password, emailNotifications, textNotifications
        });
    }

    async validateStaffCode(staffCode) {
        return await this.post('/customer/validate-staff-code', { staffCode });
    }

    async getSession() {
        return await this.get('/customer/session');
    }

    // ==================== QUEUE OPERATIONS ====================
    
    async getCurrentQueue() {
        return await this.get('/queue/current');
    }

    async getPlaceholderQueue(baseOrderNumber = 200) {
        return await this.get(`/queue/placeholder?baseOrderNumber=${baseOrderNumber}`);
    }

    async getQueuePosition(orderNumber) {
        return await this.get(`/queue/position/${orderNumber}`);
    }

    async getWaitTime(orderNumber) {
        return await this.get(`/queue/wait-time/${orderNumber}`);
    }

    // ==================== HTTP METHODS ====================
    
    async get(endpoint) {
        const response = await fetch(this.baseUrl + endpoint, {
            method: 'GET',
            headers: { 'Content-Type': 'application/json' },
            credentials: 'same-origin'
        });
        return await this.handleResponse(response);
    }

    async post(endpoint, data) {
        const response = await fetch(this.baseUrl + endpoint, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            credentials: 'same-origin',
            body: JSON.stringify(data)
        });
        return await this.handleResponse(response);
    }

    async put(endpoint, data) {
        const response = await fetch(this.baseUrl + endpoint, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            credentials: 'same-origin',
            body: JSON.stringify(data)
        });
        return await this.handleResponse(response);
    }

    async delete(endpoint) {
        const response = await fetch(this.baseUrl + endpoint, {
            method: 'DELETE',
            headers: { 'Content-Type': 'application/json' },
            credentials: 'same-origin'
        });
        return await this.handleResponse(response);
    }

    async handleResponse(response) {
        if (!response.ok) {
            const error = await response.json().catch(() => ({ message: 'Request failed' }));
            throw new Error(error.message || `HTTP ${response.status}`);
        }
        return await response.json();
    }
}

// Create global instance
const apiClient = new ApiClient();
