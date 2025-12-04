# Clipper Coffee Corner – Backend API


Team: Integration 
Members:

Xin Ding – Built most backend models and API controllers; 

Thomas Baker – Helped design API structure, created DTO classes, and wrote test classes / mock designs.

1. What this part of the project does

This project is the backend for Clipper Coffee Corner.
Our integration/API layer exposes REST endpoints that other teams (UI, Queue, Payment, Notifications) can call.

Main things we built:

Order & Menu API

Create orders (guest or logged-in user).

Get single order, all orders, or orders for a specific user.

Update order status (Pending → Completed, etc.).

Notification-focused endpoints

Order item details for one order
GET /api/orders/{orderId}/items-detail
Returns a list of items in the order with:

menuItemId, menuItemName

list of option names (e.g., “Breve (half and half)”, “Small”)

quantity, unitPrice, lineTotal

Popular items in the last N orders
GET /api/orders/popular-items?n=10
Returns aggregated info per menu item:

menuItemId, menuItemCategoryId, menuItemName

totalQuantity

ordersCount (how many orders contained this item)

Square payment integration

POST /api/checkout/{orderId}/payment-link
Creates a Square payment link for a given order using Square’s Checkout / Online Checkout API.
Square
+1

Returns JSON like:

{
  "orderId": 43,
  "paymentLink": "https://sandbox.square.link/u/xxxxxx"
}


Support code

Models for orders, order items, menu items, combinations, option groups/values, users, passwords, etc.

DTOs under Models/Dtos for clean responses to other teams.

SquareCheckoutService: small service that wraps the Square HTTP call.

Optional wwwroot/test-payment.html page to quickly test the payment API from a browser.

2. Concept behind the features

The app follows a layered design:

Database (tables, EF Core models)

Integration / API (this team): REST endpoints and DTOs

Other teams (UI, Queue, Notifications, Payment) call our APIs instead of touching the DB directly.

DTOs keep responses small and stable. If DB changes later, UI code doesn’t need to change as long as the DTO stays the same.

For payments we wanted a simple, safe flow:

Our API does not store credit cards.

We only create a Square-hosted checkout link and send the URL back to the UI. The real payment happens on Square.

3. What each person worked on

Xin Ding

Built most of the backend models and API controllers, including:

OrdersController – create orders, list orders, get by id, update status.

Notification endpoints:

/api/orders/{orderId}/items-detail

/api/orders/popular-items

Integrated the API with Entity Framework Core (AppDbContext, LINQ queries, includes, grouping).

Implemented Square integration:

Configured Program.cs with AddHttpClient("Square", …) and ISquareCheckoutService.

Implemented SquareCheckoutService to call
POST /v2/online-checkout/payment-links on Square.
Square
+1

Created CheckoutController endpoint /api/checkout/{orderId}/payment-link.

Added test-payment.html to quickly call the API from the browser.

Wrote and ran manual API tests using PowerShell and browser (for example Invoke-Safe script).

Thomas Baker

Helped design the API structure and contract with other teams (API sheet).

Created DTO classes such as:

OrderItemDetailsDto

PopularItemDto

Built test classes and mocks to exercise the controllers and services.

Helped review LINQ queries and edge cases.

4. Challenges and how we fixed them

Square authentication errors

Problem: first calls to Square returned AUTHENTICATION_ERROR / UNAUTHORIZED.

Cause: wrong access token / location ID and using placeholders in appsettings.json.

Fix:

Move real Square sandbox values into appsettings.Development.json under "Square": { … }.

Keep appsettings.json with placeholder values so no secrets go to Git.

Make sure the base URL is https://connect.squareupsandbox.com.

LINQ & EF Core grouping for popular items

Problem: grouping by menu item and counting orders was tricky, and we also needed to include menu category.

Fix:

Wrote a LINQ query that:

filters order items by the last N orders,

GroupBy on MenuItemId, MenuItemCategoryId, MenuItemName,

sums Quantity and counts distinct OrderIds.

Null reference warnings

Problem: EF navigation properties (Combination, MenuItem, CombinationOptions) are nullable at compile time.

Fix:

Used null-checks and fallback values in projections (menuItem?.Name ?? "Unknown").

This keeps runtime safe while still working with EF.

Git rebase conflicts

Problem: we accidentally got into “Rebase in progress” with conflicts in appsettings.json and Program.cs.

Fix:

Resolved conflicts by keeping our updated API + Square code.

Completed the rebase and tested all endpoints again.

5. What worked well / what didn’t

Worked well

Using DTOs made it easy to give Notification and UI teams exactly what they asked for.

The PowerShell Invoke-Safe helper was very fast for testing endpoints.

Square Checkout API is easy once configuration is correct: one POST → get payment link.

Didn’t work at first

Using Square with placeholder tokens; we learned to separate secrets into appsettings.Development.json.

Some first LINQ queries returned duplicate or wrong counts; needed better grouping and Distinct() calls.

Git rebase caused some confusion; we had to learn how to fix it in Visual Studio.

6. Special configurations / differences in approach

Database connection

For class demo we use localdb:

"DefaultConnection": "Server=(localdb)\\\\MSSQLLocalDB;Database=ClipperCoffeeCorner;Trusted_Connection=True;TrustServerCertificate=True;"


The same code also works with Azure SQL by changing the connection string.

Square configuration

appsettings.json (checked into Git, safe):

"Square": {
  "AccessToken": "SANDBOX_ACCESS_TOKEN_PLACEHOLDER",
  "LocationId": "SANDBOX_LOCATION_ID_PLACEHOLDER",
  "ApiVersion": "2023-08-16"
}


appsettings.Development.json (local only):

"Square": {
  "AccessToken": "<real sandbox access token>",
  "LocationId": "<real sandbox location id>",
  "ApiVersion": "2023-08-16",
  "BaseUrl": "https://connect.squareupsandbox.com"
}


Before pushing to GitHub: check that only placeholder values are in appsettings.json.
Do not commit real keys.

HttpClient setup in Program.cs

builder.Services.AddHttpClient("Square", client =>
{
    var baseUrl = builder.Configuration["Square:BaseUrl"]
                  ?? "https://connect.squareupsandbox.com";
    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddScoped<ISquareCheckoutService, SquareCheckoutService>();

7. How to run and test (step by step)
7.1 Run the API

Open solution in Visual Studio 2022.

Make sure ClipperCoffeeCorner is the startup project.

Confirm appsettings.json has a valid localdb connection string.

Confirm appsettings.Development.json has Square sandbox values.

Run the project (F5). Note the HTTPS URL, e.g.
https://localhost:7001.

7.2 Quick API tests (PowerShell)

Use this once per session:

function Invoke-Safe {
    param(
        [Parameter(Mandatory)][string]$Method,
        [Parameter(Mandatory)][string]$Uri,
        [object]$Body = $null
    )

    try {
        if ($Body -ne $null) {
            Invoke-RestMethod -Method $Method -Uri $Uri `
                -ContentType "application/json" -Body $Body
        } else {
            Invoke-RestMethod -Method $Method -Uri $Uri
        }
    }
    catch {
        $resp = $_.Exception.Response
        if ($resp) {
            $reader = New-Object System.IO.StreamReader($resp.GetResponseStream())
            $reader.ReadToEnd()
        } else {
            $_.Exception.Message
        }
    }
}

$base = "https://localhost:7001"


Create a guest order

$body = @{
    userId = $null
    items  = @(
        @{ combinationId = 1; quantity = 1 }
    )
} | ConvertTo-Json -Depth 3

$order = Invoke-Safe -Method Post -Uri "$base/api/orders" -Body $body
$order


Get full order with items

Invoke-Safe -Method Get -Uri "$base/api/orders/$($order.orderId)" |
  ConvertTo-Json -Depth 5


Order items for notifications

Invoke-Safe -Method Get -Uri "$base/api/orders/$($order.orderId)/items-detail" |
  ConvertTo-Json -Depth 5


Popular items in last 10 orders

Invoke-Safe -Method Get -Uri "$base/api/orders/popular-items?n=10" |
  ConvertTo-Json -Depth 5


Create payment link

$checkout = Invoke-Safe -Method Post -Uri "$base/api/checkout/$($order.orderId)/payment-link"
$checkout | ConvertTo-Json -Depth 5


Open the paymentLink URL in a browser to see the Square checkout page.

7.3 Optional browser test page

Navigate to https://localhost:7001/test-payment.html.

Enter an orderId that exists.

Click “Create Payment Link”.

The page will show the JSON response from /api/checkout/{orderId}/payment-link.

8. Resources for future students

Square Checkout / Online payment links docs (high-level overview).
Square

Square API reference for Online Checkout payment links.
Square

Official ASP.NET Core docs for DbContext configuration with AddDbContext and connection strings.
