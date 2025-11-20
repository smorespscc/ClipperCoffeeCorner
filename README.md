# Clipper Coffee Corner – Backend

This project is the backend for the Clipper Coffee Corner app.  
It exposes REST APIs for menu data and customer orders.

## Requirements

- .NET 8 SDK
- SQL Server (localdb or Azure SQL)
- Visual Studio 2022 or `dotnet` CLI

## Database configuration

Connection string is in `appsettings.json`:

```jsonc
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:cafecorner.database.windows.net,1433;Database=Cafe Corner;User ID=cafeuser;Password=...;Encrypt=True;TrustServerCertificate=False;"
  }
}
For local development you can swap this to localdb instead:
"DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=ClipperCoffeeCorner;Trusted_Connection=True;TrustServerCertificate=True;"

The app uses this connection string in Program.cs:
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    Running the API

Start the project in Visual Studio (or dotnet run).

Check the HTTPS URL in the console (for example: https://localhost:7001).

Use that URL as the base for all API calls.

Quick PowerShell test script
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
Example API calls

All menu categories:
Invoke-Safe -Method Get -Uri "$base/api/menu/categories" | ConvertTo-Json -Depth 3

All menu items:
Invoke-Safe -Method Get -Uri "$base/api/menu/items" | ConvertTo-Json -Depth 3

Create a guest order (no user):
$body = @{
    userId = $null
    items  = @(
        @{ combinationId = 1; quantity = 1 }
    )
} | ConvertTo-Json -Depth 3

Invoke-Safe -Method Post -Uri "$base/api/orders" -Body $body

Create an order for logged-in user (example UserId = 1 in dbo.User):
$body = @{
    userId = 1
    items  = @(
        @{ combinationId = 1; quantity = 1 }
    )
} | ConvertTo-Json -Depth 3

Invoke-Safe -Method Post -Uri "$base/api/orders" -Body $body

Get all orders:
Invoke-Safe -Method Get -Uri "$base/api/orders" | ConvertTo-Json -Depth 3

Get orders for a specific user:
Invoke-Safe -Method Get -Uri "$base/api/orders?userId=1" | ConvertTo-Json -Depth 3

Update order status:
$body = @{ status = "Completed" } | ConvertTo-Json
Invoke-Safe -Method Put -Uri "$base/api/orders/4/status" -Body $body


---
