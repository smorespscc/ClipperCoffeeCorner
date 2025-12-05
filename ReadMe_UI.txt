================================================================================
                    CLIPPER COFFEE CORNER - UI DOCUMENTATION
                         User Interface Implementation Guide
================================================================================

PROJECT SUMMARY
================================================================================
The Clipper Coffee Corner UI is a modern, responsive web-based ordering system 
for a campus coffee shop. Built using ASP.NET Core MVC with Razor views, the 
interface provides an intuitive, mobile-friendly experience for customers to 
browse menus, customize orders, and track their queue position.

The UI features a coffee-themed design with smooth animations, real-time menu 
availability based on service hours (Breakfast: 8:00am-10:30am, Lunch: 
10:30am-2:00pm), and comprehensive order management capabilities including 
saved orders, recent order history, and staff discount support.


CORE UI FEATURES & PAGES
================================================================================

1. LOGIN PAGE (Index.cshtml)
   - Clean authentication interface with email/password fields
   - Staff login option with code verification (test code: 9999)
   - Form validation with real-time feedback
   - Success modal with automatic redirect to menu
   - Idle timeout (60 seconds) redirects to queue page
   - Debug mode activation via special email: spscc@edu.com

2. REGISTRATION PAGE (Register.cshtml)
   - Comprehensive user registration form
   - Real-time password strength indicator (weak/fair/good/strong)
   - Password requirements validation:
     * Minimum 8 characters
     * One uppercase letter
     * One lowercase letter
     * One number
     * One special character
   - Notification preferences (email/SMS consent)
   - Phone number and email validation
   - Data stored in localStorage for demo purposes

3. MENU PAGE (Menu.cshtml) - PRIMARY ORDERING INTERFACE
   - Dynamic time-based menu system with automatic service period detection
   - Visual menu sections:
     * Recent Orders - carousel of previously ordered items
     * Saved Orders - customer's favorite order configurations
     * Trending Items - popular menu selections
     * Specials - featured items
     * Drink Menu - full beverage selection
     * Food Menu - complete food offerings
   
   - Service Period Management:
     * Breakfast menu (8:00am - 10:30am)
     * Lunch menu (10:30am - 2:00pm)
     * Automatic redirect to "Closed" page outside hours
     * Viewing mode for browsing menus when closed
     * Debug mode bypasses time restrictions
   
   - Item Customization Modal:
     * Multi-tab interface for ordering multiple quantities
     * Drink modifiers: temperature, milk type, flavors, extras
     * Food modifiers: cooking level, sides, dietary restrictions
     * Special requests text input with microphone support
     * Visual selection indicators (green checkmark for manual, gold star for saved)
     * Real-time quantity badges
   
   - Smart Features:
     * Items unavailable during current period shown as grayscale
     * Saved orders can be applied, edited, or deleted
     * Recent orders can be reordered with one click
     * Selection legend shows manual vs. saved order items
     * Responsive carousel navigation with touch support

4. CHECKOUT PAGE (Checkout.cshtml)
   - Comprehensive order summary table
   - Columns: Item, Quantity, Modifiers, Special Requests, Price, Action
   - Individual item removal buttons
   - "Clear All" button for cart reset
   - Staff discount display (10% when applicable)
   - Subtotal and total calculations
   - Navigation: Back to Menu or Proceed to Payment

5. PAYMENT PAGE (Payment.cshtml)
   - Order review with final pricing
   - Mock payment form (card number, expiry, CVV)
   - Form validation for payment fields
   - Success/Error modals with appropriate feedback
   - Automatic redirect to queue after successful payment
   - Order data saved to recent orders history

6. QUEUE PAGE (Queue.cshtml)
   - Real-time order status display
   - User's order highlighted in queue table
   - Position in line and estimated wait time
   - Status badges: Queued (gray), Preparing (yellow), Ready (green)
   - Placeholder queue entries for context
   - Auto-refresh every 10 seconds
   - "Simulate Ready" button for testing
   - "New Order" button to start fresh
   - Wake-up functionality from idle/resting mode

7. CLOSED PAGE (Closed.cshtml)
   - Friendly "currently closed" message
   - Service hours display
   - Options to view Breakfast or Lunch menu in preview mode
   - Coffee-themed icon and styling
   - "Back to Home" navigation

8. CONTRIBUTIONS PAGE (Contributions.cshtml)
   - Interactive team member showcase
   - Expandable accordion-style contributor cards
   - Organized by team: Database, API Controllers, UI, Payments, Notifications
   - Special thanks section for instructor
   - Responsive design with mobile optimization


UI DESIGN CONCEPT & PHILOSOPHY
================================================================================

VISUAL THEME:
The interface uses a warm, coffee-inspired color palette:
- Primary: Coffee brown (#6f4e37, #3b2f2f)
- Accent: Cream and tan (#f5f3f0, #c4a484)
- Highlights: Orange-red gradients (#ff7043, #ff5722)
- Success: Green (#28a745)
- Warning: Yellow (#ffc107)

DESIGN PRINCIPLES:
1. Mobile-First Responsive Design
   - All pages adapt seamlessly to phone, tablet, and desktop
   - Touch-friendly controls with appropriate sizing
   - Collapsible navigation and optimized layouts

2. Progressive Enhancement
   - Core functionality works without JavaScript
   - Enhanced features layer on top (animations, real-time updates)
   - Graceful degradation for older browsers

3. User Feedback & Validation
   - Real-time form validation with clear error messages
   - Success/error modals for important actions
   - Custom notification system for non-blocking alerts
   - Visual indicators for selected items and states

4. Accessibility Considerations
   - Semantic HTML structure
   - ARIA labels for screen readers
   - Keyboard navigation support
   - High contrast text and interactive elements
   - Focus indicators on form fields

5. Performance Optimization
   - Lazy loading of images
   - Efficient localStorage usage
   - Minimal external dependencies
   - CSS animations using hardware acceleration


TECHNICAL IMPLEMENTATION
================================================================================

FRONTEND STACK:
- ASP.NET Core MVC 8.0 with Razor Views
- Bootstrap 5.3.3 (CSS framework)
- Bootstrap Icons 1.11.3
- Vanilla JavaScript (ES6+)
- CSS3 with custom properties and animations
- LocalStorage for client-side data persistence

FILE STRUCTURE:
Views/
├── Home/
│   ├── Index.cshtml (Login)
│   ├── Register.cshtml
│   ├── Menu.cshtml
│   ├── Checkout.cshtml
│   ├── Payment.cshtml
│   ├── Queue.cshtml
│   ├── Closed.cshtml
│   └── Contributions.cshtml
├── Shared/
│   ├── _Layout.cshtml (Standard layout)
│   └── _CoffeeLayout.cshtml (Coffee-themed layout)
└── _ViewImports.cshtml

wwwroot/
├── css/
│   ├── global.css (Shared coffee theme)
│   ├── index.css (Login page)
│   ├── register.css
│   ├── menu.css (Largest, most complex)
│   ├── checkout.css
│   ├── payment.css
│   ├── queue.css
│   └── closed.css
└── js/
    ├── index.js (Login logic)
    ├── register.js (Registration validation)
    ├── menu.js (2086 lines - core ordering logic)
    ├── checkout.js (Cart management)
    ├── payment.js (Payment processing)
    ├── queue.js (Queue display)
    └── site.js (Global utilities)

KEY JAVASCRIPT FEATURES:
- Time-based menu management with Pacific timezone detection
- Multi-tab order customization system
- LocalStorage-based cart and order persistence
- Saved orders with CRUD operations
- Recent orders tracking (last 10)
- Staff discount calculation
- Form validation and error handling
- Modal management with Bootstrap
- Carousel navigation and item selection
- Notification system
- Idle timeout and wake functionality


CHALLENGES FACED & SOLUTIONS
================================================================================

CHALLENGE 1: Time-Based Menu Availability
Problem: Menu items needed to be available only during specific service periods,
but users should still be able to browse when closed.

Solution: Implemented a three-mode system:
- Active mode: Full ordering during service hours
- Viewing mode: Browse-only when closed (via URL parameter)
- Debug mode: Bypass restrictions for testing (spscc@edu.com login)

Items outside current period are shown as grayscale with hover tooltips 
explaining availability. Carousels automatically reorder to show available 
items first.

CHALLENGE 2: Complex Order Customization
Problem: Users needed to order multiple quantities of the same item with 
different customizations (e.g., 3 lattes with different milk types).

Solution: Created a tab-based system within the modal where each tab represents
one instance of the item. Users can:
- Add new tabs for additional quantities
- Switch between tabs to customize each individually
- Close tabs to remove instances
- See all customizations before applying to cart

This required sophisticated state management to track modifiers and special 
requests per tab, with proper saving/loading when switching tabs.

CHALLENGE 3: Saved Orders vs. Manual Selection
Problem: Users needed to distinguish between items added manually and items 
from applied saved orders, with ability to modify saved orders.

Solution: Implemented a dual-indicator system:
- Green checkmark: Manually selected items
- Gold star: Items from applied saved orders
- Selection legend in header explains the difference
- Saved orders can be edited (opens modal with "Update" button)
- Applied saved orders tracked separately in localStorage

CHALLENGE 4: Mobile Responsiveness
Problem: Complex carousel layouts and multi-column designs didn't work well 
on small screens.

Solution: Extensive media queries and responsive design:
- Stacked layouts on mobile
- Touch-friendly carousel controls
- Collapsible sections
- Optimized font sizes and spacing
- Tab width calculations based on container size
- Hidden helper text on mobile to save space

CHALLENGE 5: Form Validation & User Feedback
Problem: Users needed immediate feedback on form errors without page reloads.

Solution: Implemented client-side validation with:
- Real-time password strength indicator
- Individual field validation on blur/input
- Visual feedback (red borders, error messages)
- Success modals for completed actions
- Custom notification system for non-blocking alerts
- Proper error handling with try-catch blocks

CHALLENGE 6: LocalStorage Data Management
Problem: Multiple pages needed to share cart, order, and user data without 
a backend database.

Solution: Structured localStorage schema:
- selectedItems: Current cart contents
- savedOrders: User's favorite orders
- recentOrders: Last 10 completed orders
- appliedSavedOrders: Track which saved orders are in cart
- userData: User profile information
- isStaff/staffCode: Staff discount eligibility
- debugMode: Testing mode flag

All data stored as JSON with proper serialization/deserialization and error 
handling for corrupted data.


WHAT WORKED WELL
================================================================================

✓ Coffee-Themed Design: The warm color palette and smooth animations create 
  an inviting, cohesive experience that matches the café atmosphere.

✓ Carousel Navigation: Bootstrap carousels with custom styling provide an 
  elegant way to browse menu items with touch support.

✓ Tab-Based Customization: The multi-tab system for ordering multiple 
  quantities is intuitive and powerful, solving a complex UX problem.

✓ Visual Feedback: Selection indicators, quantity badges, and status colors 
  make the interface state immediately clear to users.

✓ Responsive Design: The interface works seamlessly across devices from 
  phones to desktops with appropriate adaptations.

✓ LocalStorage Persistence: Cart and order data survives page refreshes and 
  browser sessions, providing a smooth user experience.

✓ Time-Based Logic: Automatic menu switching and availability checking 
  reduces staff workload and prevents invalid orders.

✓ Accessibility: Semantic HTML, ARIA labels, and keyboard navigation make 
  the interface usable for diverse users.


WHAT DIDN'T WORK / AREAS FOR IMPROVEMENT
================================================================================

✗ No Backend Integration: All data stored in localStorage means:
  - Data lost if browser cache cleared
  - No synchronization across devices
  - No real payment processing
  - No actual queue management
  - Security concerns with client-side storage

✗ Limited Error Recovery: If localStorage data becomes corrupted, the app 
  may break. Need better error boundaries and data validation.

✗ No Real-Time Updates: Queue page uses simulated data and manual refresh. 
  A real implementation would need WebSockets or SignalR for live updates.

✗ Mock Payment: Payment form is purely cosmetic with random success/failure. 
  Real integration with Square or Stripe would be needed for production.

✗ No Image Optimization: Menu item images loaded from Unsplash CDN can be 
  slow. Should use optimized, locally hosted images with proper sizing.

✗ Limited Accessibility Testing: While basic accessibility features are 
  present, comprehensive testing with screen readers and keyboard-only 
  navigation hasn't been performed.

✗ No Automated Tests: The UI lacks unit tests, integration tests, or E2E 
  tests. Manual testing only.

✗ Browser Compatibility: Primarily tested in Chrome. May have issues in 
  older browsers or Safari due to modern JavaScript features.


SPECIAL CONFIGURATIONS & DIFFERENCES
================================================================================

DUAL LAYOUT SYSTEM:
The project uses two layouts:
1. _Layout.cshtml: Standard ASP.NET layout with navbar (unused in main flow)
2. _CoffeeLayout.cshtml: Custom coffee-themed layout without navbar
   - Used by all main ordering pages
   - Includes global.css for consistent theming
   - Contributors link in bottom-right corner

DEBUG MODE:
Activated by logging in with email: spscc@edu.com
- Bypasses time restrictions (all menus always available)
- All items shown as available regardless of service period
- Useful for testing and demonstrations
- Stored in localStorage as 'debugMode' flag

STAFF DISCOUNT:
- Activated by checking "I am Staff" on login
- Requires staff code (test code: 9999)
- Applies 10% discount to all orders
- Shown separately in checkout and payment summaries
- Stored in localStorage as 'isStaff' and 'staffCode'

VIEWING MODE:
- Accessed via URL parameters: ?view=breakfast or ?view=lunch
- Allows browsing menus when café is closed
- Disables ordering functionality
- Shows yellow banner: "Viewing Menu Only - We're Currently Closed"
- Hides special requests section and footer buttons


GETTING STARTED - STEP-BY-STEP TUTORIAL
================================================================================

FOR DEVELOPERS:

1. PREREQUISITES:
   - .NET 8.0 SDK or later
   - Visual Studio 2022 or VS Code with C# extension
   - Modern web browser (Chrome, Firefox, Edge)

2. CLONE AND BUILD:
   ```
   git clone <repository-url>
   cd ClipperCoffeeCorner
   dotnet restore
   dotnet build
   ```

3. RUN THE APPLICATION:
   ```
   dotnet run
   ```
   Or press F5 in Visual Studio

4. ACCESS THE UI:
   Navigate to: https://localhost:5001 (or http://localhost:5000)

5. TEST THE FLOW:
   a) Register a new account (or use existing credentials)
   b) Login (check "I am Staff" and use code 9999 for discount)
   c) Browse menu and customize items
   d) Add items to cart
   e) Proceed to checkout
   f) Complete payment (any valid-format card works)
   g) View queue status

6. TEST DEBUG MODE:
   - Login with email: spscc@edu.com
   - All menus will be available regardless of time
   - Useful for testing outside service hours

7. TEST VIEWING MODE:
   - Navigate to: /Home/Menu?view=breakfast
   - Or: /Home/Menu?view=lunch
   - Menus shown but ordering disabled

8. INSPECT LOCALSTORAGE:
   - Open browser DevTools (F12)
   - Go to Application > Local Storage
   - View stored data: selectedItems, savedOrders, userData, etc.

FOR NEXT COHORT:

1. UNDERSTANDING THE CODEBASE:
   - Start with Views/Home/Index.cshtml (login page)
   - Follow the flow: Login → Menu → Checkout → Payment → Queue
   - Read menu.js carefully - it's the most complex file
   - Study global.css for theming approach

2. COMMON MODIFICATIONS:
   - Add new menu items: Update menu.cshtml carousels
   - Change colors: Modify CSS variables in global.css
   - Add modifiers: Update checkboxIdToLabelMap in menu.js
   - Adjust service hours: Modify time constants in menu.js

3. DEBUGGING TIPS:
   - Use browser console for JavaScript errors
   - Check localStorage for data issues
   - Use debugMode for testing outside hours
   - Bootstrap modals can be tricky - check z-index issues

4. HELPFUL RESOURCES:
   - Bootstrap 5 Docs: https://getbootstrap.com/docs/5.3/
   - ASP.NET Core MVC: https://docs.microsoft.com/aspnet/core/mvc/
   - JavaScript LocalStorage: https://developer.mozilla.org/en-US/docs/Web/API/Window/localStorage
   - CSS Flexbox: https://css-tricks.com/snippets/css/a-guide-to-flexbox/


INDIVIDUAL CONTRIBUTIONS
================================================================================

JORDAN MCNAIR - UI TEAM (FRONTEND)
Primary Responsibilities:
- Accomplished the following using Claude AI via the Kiro IDE...
    - Designed and implemented the coffee-themed visual system
    - Created ordering flow pages (Login, Menu, Checkout, Payment, Queue)
    - Developed the complex menu.js with 2000+ lines of ordering logic
    - Implemented the multi-tab customization system
    - Built the carousel-based menu navigation
    - Created the selection indicator system (checkmarks and stars)
    - Designed responsive layouts for mobile and desktop
    - Implemented form validation and user feedback systems
    - Developed the time-based menu availability logic
    - Created custom CSS animations and transitions
    - Implemented saved orders and recent orders features

Specific Files Created/Modified:
- Views/Home/Index.cshtml, Menu.cshtml, Checkout.cshtml, Payment.cshtml, Queue.cshtml
- wwwroot/js/menu.js (2086 lines), index.js, checkout.js, payment.js, queue.js
- wwwroot/css/global.css, menu.css, index.css, checkout.css, payment.css, queue.css
- Views/Shared/_CoffeeLayout.cshtml

Technical Achievements:
- Sophisticated state management with localStorage
- Complex tab-based UI for order customization
- Real-time form validation with visual feedback
- Responsive carousel navigation with touch support
- Time-zone aware service period detection
- Dual-indicator system for order tracking

NILS JULNES - UI TEAM (ARCHITECTURE)
Primary Responsibilities:
- Backend database design and schema
- Layer communication architecture between UI, API, and database
- API design and endpoint structure
- Process diagrams and system documentation
- UI development support and integration
- AI prompt consulting for code generation
- Technical architecture decisions

Specific Contributions:
- Designed the service layer interfaces (IMenuService, IOrderService, etc.)
- Created the DTO structure for data transfer
- Established the MVC pattern implementation
- Documented the system architecture
- Provided technical guidance on best practices
- Assisted with debugging complex UI issues
- Helped integrate frontend with backend services

Technical Achievements:
- Clean separation of concerns in architecture
- RESTful API design principles
- Dependency injection setup
- Service layer abstraction
- Data flow optimization

TARAN TRIGGS - UI TEAM (ADMIN, TESTS, & REQUIREMENTS)
Primary Responsibilities:
- Frontend admin interface development
- User interface components for staff/admin features
- Test management and quality assurance
- UI testing and validation
- Primary source for data on the Clipper Cafe software requirements

Specific Contributions:
- Developed admin-specific UI components
- Created staff login and discount features
- Implemented test scenarios for UI functionality
- Validated user flows and edge cases
- Documented testing procedures
- Assisted with responsive design testing

Technical Achievements:
- Staff authentication UI
- Admin dashboard components
- Test case documentation
- Quality assurance processes


TESTING DOCUMENTATION
================================================================================


MANUAL TEST SCENARIOS PERFORMED:
1. User registration with various input combinations
2. Login with valid/invalid credentials
3. Staff login with correct/incorrect codes
4. Menu browsing during different time periods
5. Item customization with various modifiers
6. Cart operations (add, remove, clear)
7. Saved order creation, editing, deletion
8. Checkout flow with staff discount
9. Payment processing (success and failure paths)
10. Queue display and refresh
11. Responsive design on multiple devices
12. Browser compatibility (Chrome, Firefox, Edge)

RECOMMENDED TEST COVERAGE:
- Unit tests for JavaScript functions (menu.js, checkout.js, etc.)
- Integration tests for page flows
- E2E tests using Selenium or Playwright
- Accessibility tests with axe-core
- Performance tests for page load times
- Cross-browser compatibility tests


FUNCTIONAL UI TEST CASE (EXECUTABLE)
================================================================================

Below is a comprehensive test case that can be executed manually or automated
with Selenium/Playwright to validate the core UI functionality:

TEST CASE: Complete Order Flow with Customization
--------------------------------------------------

OBJECTIVE: Verify that a user can register, login, customize orders, apply
staff discount, and complete payment successfully.

PRECONDITIONS:
- Application is running on localhost
- Browser localStorage is cleared
- Current time is within service hours (or debug mode enabled)

TEST STEPS:

1a. REGISTRATION
   a) Navigate to https://localhost:5001
   b) Click "Don't have an account? → Register"
   c) Fill in registration form:
      - Username: "TestUser123"
      - Phone: "555-123-4567"
      - Email: "testuser@example.com"
      - Password: "Test@1234"
      - Confirm Password: "Test@1234"
      - Check "Email notifications"
   d) Click "Register" button
   
   EXPECTED RESULT:
   - Password strength indicator shows "Strong password"
   - All requirements show green checkmarks
   - Success modal appears
   - Redirects to login page after 2 seconds

1b. LOGIN AS DEBUG ACCOUNT *optional*
(⚠ NOTE: this allows you to test breakfast/lunch menu at any time of day. Do NOT forget to DELETE THIS backdoor before deploying the final web application.)
   a) Enter email: "spscc@edu.com"
   b) Enter password: "password"
   c) Click "Login" button

2. LOGIN WITH STAFF DISCOUNT
   a) Enter email: "testuser@example.com"
   b) Enter password: "Test@1234"
   c) Check "I am Staff" checkbox
   d) Enter staff code: "9999" (⚠ NOTE: This is for debug purposes. Do NOT forget to DELETE THIS backdoor before deploying the final web application.)
   e) Click "Login" button
   
   EXPECTED RESULT:
   - Success modal appears
   - Redirects to menu page after 2 seconds
   - Staff discount flag stored in localStorage

3. BROWSE AND CUSTOMIZE FIRST ITEM
   a) Verify menu page loads with current service period displayed
   b) Click on "Cappuccino" image in Recent Orders or Drink Menu
   c) Modal opens with customization options
   d) Select modifiers:
      - Temperature: "Hot"
      - Milk: "Oat"
      - Flavor: "Vanilla"
      - Extra: "Whip Cream"
   e) Enter special request: "Extra hot please"
   f) Click "Apply" button
   
   EXPECTED RESULT:
   - Modal closes
   - Cappuccino image shows green checkmark indicator
   - Quantity badge shows "1"
   - Item added to localStorage selectedItems

4. ADD MULTIPLE QUANTITIES WITH DIFFERENT CUSTOMIZATIONS
   a) Click on "Cappuccino" image again
   b) Modal opens showing previous customization
   c) Click "+ Add Another" button to create second tab
   d) Switch to second tab
   e) Select different modifiers:
      - Temperature: "Iced"
      - Milk: "Almond"
      - Flavor: "Caramel"
   f) Click "Apply" button
   
   EXPECTED RESULT:
   - Modal closes
   - Quantity badge updates to "2"
   - Two separate items in localStorage with different modifiers

5. SAVE ORDER AS FAVORITE
   a) Click on "Cappuccino" image again
   b) Click "Save Order" button in modal
   c) Enter name: "My Morning Coffee"
   d) Click OK in prompt
   
   EXPECTED RESULT:
   - Success notification appears
   - Modal closes
   - Saved order appears in "Saved Orders" carousel
   - Gold star indicator on saved order image

6. ADD FOOD ITEM
   a) Click on "Breakfast Sandwich" in Food Menu
   b) Modal opens with food options
   c) Select modifiers:
      - Cooking: "Well Done"
      - Extras: "Extra Cheese", "Bacon"
   d) Enter special request: "No onions"
   e) Click "Apply" button
   
   EXPECTED RESULT:
   - Modal closes
   - Breakfast Sandwich shows green checkmark
   - Quantity badge shows "1"

7. PROCEED TO CHECKOUT
   a) Click "Proceed to Checkout" button in footer
   b) Verify checkout page loads
   
   EXPECTED RESULT:
   - Order summary table shows:
     * 2 Cappuccino entries with different modifiers
     * 1 Breakfast Sandwich with modifiers
   - Subtotal calculated correctly
   - Staff discount (10%) displayed
   - Total = Subtotal - Discount

8. MODIFY CART
   a) Click "Remove" button on one Cappuccino
   b) Verify item removed from table
   c) Verify totals recalculated
   d) Click "Back to Menu" button
   e) Verify menu page loads with remaining items still selected
   f) Click "Proceed to Checkout" again
   
   EXPECTED RESULT:
   - Cart persists correctly
   - Removed item stays removed
   - Remaining items still in cart

9. COMPLETE PAYMENT
   a) Click "Proceed to Payment" button
   b) Verify payment page loads with order summary
   c) Fill in payment form:
      - Card Number: "4111111111111111"
      - Expiry: "12/25"
      - CVV: "123"
   d) Click "Pay" button
   
   EXPECTED RESULT:
   - Payment processes (50% chance success in mock)
   - If success:
     * Success modal appears
     * Redirects to queue page after 2 seconds
     * Order added to recent orders
     * Cart cleared
   - If failure:
     * Error modal appears
     * Can try again

10. VIEW QUEUE
    a) Verify queue page loads
    b) Check order appears in queue table
    c) Verify position and wait time displayed
    d) Click "Refresh" button
    e) Click "Simulate Ready" button
    
    EXPECTED RESULT:
    - User's order highlighted in table
    - Order number displayed
    - Status badge shows "Queued"
    - Ready modal appears when simulated

11. START NEW ORDER
    a) Click "New Order" button
    b) Verify redirects to login page
    c) Login again
    d) Verify cart is empty
    e) Verify recent orders carousel shows previous order
    
    EXPECTED RESULT:
    - Clean slate for new order
    - Previous order in history
    - Can reorder from recent orders

12. TEST SAVED ORDER
    a) Navigate to menu page
    b) Click on saved order "My Morning Coffee" in Saved Orders carousel
    c) Modal opens with saved configuration
    d) Verify all modifiers pre-selected
    e) Click "Apply" button
    f) Verify items added to cart with gold star indicator
    
    EXPECTED RESULT:
    - Saved order applied correctly
    - All customizations preserved
    - Gold star indicator shows it's from saved order

13. TEST VIEWING MODE
    a) Navigate to /Home/Menu?view=breakfast
    b) Verify yellow banner appears
    c) Verify ordering buttons hidden
    d) Verify can browse but not order
    
    EXPECTED RESULT:
    - Viewing mode active
    - No ordering functionality
    - Menu items visible

14. CLEANUP
    a) Open browser DevTools
    b) Go to Application > Local Storage
    c) Clear all data
    d) Refresh page
    
    EXPECTED RESULT:
    - All data cleared
    - Redirects to login page

PASS CRITERIA:
- All steps complete without errors
- Data persists correctly in localStorage
- UI updates reflect state changes
- Calculations are accurate
- Navigation flows work correctly
- Modals appear and dismiss properly
- Responsive design works on mobile and desktop

AUTOMATED TEST IMPLEMENTATION:
This test case can be automated using Selenium WebDriver or Playwright with
the following structure:

```csharp
[TestClass]
public class UIFlowTests
{
    private IWebDriver driver;
    
    [TestInitialize]
    public void Setup()
    {
        driver = new ChromeDriver();
        driver.Manage().Window.Maximize();
        driver.Navigate().GoToUrl("https://localhost:5001");
        // Clear localStorage
        ((IJavaScriptExecutor)driver).ExecuteScript("localStorage.clear();");
    }
    
    [TestMethod]
    public void TestCompleteOrderFlow()
    {
        // Implement steps 1-14 using Selenium commands
        // Example:
        driver.FindElement(By.LinkText("Register")).Click();
        driver.FindElement(By.Id("username")).SendKeys("TestUser123");
        // ... continue with all steps
    }
    
    [TestCleanup]
    public void Teardown()
    {
        driver.Quit();
    }
}
```


CONCLUSION
================================================================================

The Clipper Coffee Corner UI represents a comprehensive, modern web ordering
system with sophisticated features like time-based menus, multi-tab 
customization, and saved orders. While it successfully demonstrates the 
complete user flow and provides an excellent user experience, it would benefit
from backend integration, automated testing, and production-ready payment 
processing for real-world deployment.

The codebase is well-structured and documented, making it accessible for 
future developers to understand and extend. The coffee-themed design creates
a cohesive, inviting atmosphere that enhances the ordering experience.

For questions or support, refer to the Contributors page or contact the 
development team.

================================================================================
                              END OF DOCUMENTATION
================================================================================
