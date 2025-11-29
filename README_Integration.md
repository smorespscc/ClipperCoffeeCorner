# Clipper Coffee Corner – Backend API

This project is the **backend API** for the Clipper Coffee Corner app.  
It exposes REST endpoints for:

- Menu data (categories and items)
- Customer orders
- Notification data (items in an order, popular items)
- Square **sandbox** checkout links

---

## Requirements

- .NET 8 SDK  
- SQL Server (LocalDB for dev, or Azure SQL in the cloud)  
- Visual Studio 2022 **or** `dotnet` CLI

---

## Configuration

### 1. Database connection

The API uses the `DefaultConnection` string from configuration.

**`appsettings.json` (checked into Git – safe values only):**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=ClipperCoffeeCorner;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Square": {
    "AccessToken": "SANDBOX_ACCESS_TOKEN_PLACEHOLDER",
    "LocationId": "SANDBOX_LOCATION_ID_PLACEHOLDER",
    "ApiVersion": "2023-08-16"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
Note: This file must not contain real secrets.
The placeholders are fine to commit to Git.

In Program.cs the context is wired like this:

csharp
Copy code
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));
You can change the connection string to Azure SQL on your own machine if needed.

2. Square sandbox checkout
For local development, put the real sandbox keys in
appsettings.Development.json (this file is normally NOT committed or is git-ignored):

json
Copy code
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Square": {
    "AccessToken": "EAAAXXX...YOUR_SANDBOX_ACCESS_TOKEN...",
    "LocationId": "YOUR_SANDBOX_LOCATION_ID",
    "Environment": "Sandbox",
    "BaseUrl": "https://connect.squareupsandbox.com",
    "ApiVersion": "2023-08-16"
  }
}
The backend uses these values through ISquareCheckoutService and an HttpClient
registered in Program.cs:

csharp
Copy code
builder.Services.AddHttpClient("Square", client =>
{
    client.BaseAddress = new Uri("https://connect.squareupsandbox.com");
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddScoped<ISquareCheckoutService, SquareCheckoutService>();
Running the API
Open the solution in Visual Studio 2022.

Make sure the profile is set to the HTTPS Kestrel endpoint.

Press F5 or run dotnet run from the project folder.

In the console you’ll see something like:

text
Copy code
Now listening on: https://localhost:7001
Use this as the $base URL for all tests.

Quick PowerShell helper
You can use this helper to test endpoints quickly:

powershell
Copy code
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
        }
        else {
            Invoke-RestMethod -Method $Method -Uri $Uri
        }
    }
    catch {
        $resp = $_.Exception.Response
        if ($resp) {
            $reader = New-Object System.IO.StreamReader($resp.GetResponseStream())
            $reader.ReadToEnd()
        }
        else {
            $_.Exception.Message
        }
    }
}

$base = "https://localhost:7001"
Main API Endpoints
Menu
All menu categories:

powershell
Copy code
Invoke-Safe -Method Get -Uri "$base/api/menu/categories" |
    ConvertTo-Json -Depth 3
All menu items:

powershell
Copy code
Invoke-Safe -Method Get -Uri "$base/api/menu/items" |
    ConvertTo-Json -Depth 3
Orders – basic
Create a guest order (no user):

powershell
Copy code
$body = @{
    userId = $null
    items  = @(
        @{ combinationId = 1; quantity = 1 }
    )
} | ConvertTo-Json -Depth 3

Invoke-Safe -Method Post -Uri "$base/api/orders" -Body $body
Create an order for a logged-in user (example UserId = 1):

powershell
Copy code
$body = @{
    userId = 1
    items  = @(
        @{ combinationId = 1; quantity = 1 }
    )
} | ConvertTo-Json -Depth 3

Invoke-Safe -Method Post -Uri "$base/api/orders" -Body $body
Get all orders:

powershell
Copy code
Invoke-Safe -Method Get -Uri "$base/api/orders" |
    ConvertTo-Json -Depth 3
Get orders for a specific user:

powershell
Copy code
Invoke-Safe -Method Get -Uri "$base/api/orders?userId=1" |
    ConvertTo-Json -Depth 3
Update order status:

powershell
Copy code
$body = @{ status = "Completed" } | ConvertTo-Json
Invoke-Safe -Method Put -Uri "$base/api/orders/4/status" -Body $body
Get full order details by id:

powershell
Copy code
Invoke-Safe -Method Get -Uri "$base/api/orders/4" |
    ConvertTo-Json -Depth 5
Notification Team Endpoints
These are the two endpoints added specifically for the notification team.

1. Items in a specific order
GET /api/orders/{orderId}/items-detail

Returns, for each line in the order:

menuItemId

menuItemName

optionNames (list of option names only)

quantity

unitPrice

lineTotal

Example:

powershell
Copy code
$details = Invoke-Safe -Method Get `
    -Uri "$base/api/orders/43/items-detail"

$details | ConvertTo-Json -Depth 5
Response shape (example):

json
Copy code
[
  {
    "menuItemId": 1,
    "menuItemName": "Hot Latte",
    "optionNames": [ "Breve (half and half)", "Small" ],
    "quantity": 1,
    "unitPrice": 4.50,
    "lineTotal": 4.50
  }
]
2. Popular items in the last n orders
GET /api/orders/popular-items?n=10

Aggregates the last n orders and returns one row per unique menu item:

menuItemId

menuItemCategoryId

menuItemName

totalQuantity (sum of quantities across the N orders)

ordersCount (how many orders contained that menu item)

Example:

powershell
Copy code
$popular = Invoke-Safe -Method Get `
    -Uri "$base/api/orders/popular-items?n=10"

$popular | ConvertTo-Json -Depth 5
Example response:

json
Copy code
[
  {
    "menuItemId": 1,
    "menuItemCategoryId": 7,
    "menuItemName": "Hot Latte",
    "totalQuantity": 9,
    "ordersCount": 9
  },
  {
    "menuItemId": 7,
    "menuItemCategoryId": 9,
    "menuItemName": "Iced Latte",
    "totalQuantity": 1,
    "ordersCount": 1
  }
]
Payment / Checkout Endpoint (Square sandbox)
The payment team uses a dedicated endpoint that calls Square’s
Online Checkout – Create Payment Link API via SquareCheckoutService.

Endpoint
POST /api/checkout/{orderId}/payment-link

Looks up the order and its items.

Builds a Square order payload (items + prices).

Calls Square sandbox.

Returns a JSON object with the Square checkout URL.

Example PowerShell test (assuming you already created an order):

powershell
Copy code
$checkout = Invoke-Safe -Method Post `
    -Uri "$base/api/checkout/43/payment-link"

$checkout | ConvertTo-Json -Depth 5
Example response:

json
Copy code
{
  "orderId": 43,
  "paymentLink": "https://sandbox.square.link/u/eYkhEaHr"
}
You can paste paymentLink into a browser to see the Square sandbox checkout page.

Optional: Simple browser test page
There is a small static test page under wwwroot/test-payment.html
that lets you trigger the checkout API from the browser:

URL: https://localhost:7001/test-payment.html

You can type an order ID and click the button.
The page will call POST /api/checkout/{orderId}/payment-link and display the JSON.