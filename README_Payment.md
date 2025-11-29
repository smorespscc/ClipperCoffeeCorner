# Payment — Square integration (Checkout + Webhooks)

Purpose
- Document payment flow and operational details for:
  - `Services/SquareCheckoutService.cs` (`ISquareCheckoutService`)
  - `Services/SquareWebhookService.cs` (`ISquareWebhookService`)
  - `Controllers/CheckoutController.cs`
  - `Controllers/WebhookController.cs`

Quick summary
- `SquareCheckoutService.CreatePaymentLinkAsync(Order?, string? redirectUrl)` composes a Square `payment-link` payload from a persisted DB `Order` (loads authoritative items via `AppDbContext`), converts decimal dollar amounts → integer cents, posts to Square, and returns the hosted checkout URL.
- `CheckoutController` uses session key `CurrentOrder` to read a transient order and calls the checkout service; `CreateLink` redirects the browser to Square; `Success` cleans up session after return.
- `WebhookController` receives Square webhook POSTs and verifies the signature. It still needs to map events to local `Order` records and update `Order.Status` / `Order.PlacedAt`.

Required configuration
- We created a square account in order to use the sandbox environment for testing, you will likely need to do the same.
- Add these keys (store secrets securely — do not commit):
  - `Square:AccessToken` — Square access token (sandbox for local/dev).
  - `Square:LocationId` — Square location id.
  - `Square:ApiVersion` — e.g. `2023-08-16`.
  - `Square:BaseUrl` — optional override (sandbox: `https://connect.squareupsandbox.com`).
  - `ConnectionStrings:DefaultConnection` — used by `AppDbContext`.
- Register services in `Program.cs`:
  - `builder.Services.AddHttpClient("Square", ...)`
  - `builder.Services.AddScoped<ISquareCheckoutService, SquareCheckoutService>()`
  - `builder.Services.AddScoped<ISquareWebhookService, SquareWebhookService>()`
  - `builder.Services.AddDbContext<AppDbContext>(...)`

Checkout flow (runtime)
1. User triggers `GET /Checkout/CreateLink` (or a UI that calls it).
2. `CheckoutController` reads `HttpContext.Session.GetObject<Order>("CurrentOrder")`.
   - If you persist only a transient order in session, the service may load authoritative details from DB using `Order.OrderId`. Ensure the `Order` exists if you pass an ID.
3. `SquareCheckoutService` maps each `OrderItem`:
   - `name` ← `Combination.Code` or fallback,
   - `quantity` as string,
   - `base_price_money.amount` ← `Convert.ToInt64(Math.Round(UnitPrice * 100m))`,
   - `currency` = `"USD"`.
4. The service posts `/v2/online-checkout/payment-links` and returns the `payment_link.url`.
5. Controller redirects user to the returned Square URL.
6. Square redirects back to the `redirect_url` you supplied (commonly `Checkout/Success`, currently `Home/Index`).

Webhook flow (recommended)
- Typical route: `POST /api/webhook` handled by `WebhookController`.
- Incoming steps:
  1. Verify the webhook signature header (Square sends a signature header — validate per Square docs).
  2. Parse event JSON and map to domain actions:
     - Update `Order.Status` (e.g., to `Placed`) and set `PlacedAt` when appropriate.
  3. Persist changes and return `200 OK`.
- Security: DO NOT process events until signature verification succeeds.

Database mappings (notes)
- `OrderItem.LineTotal` is computed in SQL (`Quantity * UnitPrice`) — supply `Quantity` and `UnitPrice` only.
- We did not intend on storing Payment information, square keeps records that are queryable via their dashboard and APIs.
- Correlate Square `reference_id` (set as `order.reference_id` in the payload) to `Order.OrderId` for reliable mapping.

Local testing (sandbox
- Use Square sandbox credentials and set `Square:BaseUrl` to `https://connect.squareupsandbox.com`.
- To test webhooks locally:
  - Expose your local app to the internet (e.g., `ngrok http 5000`).
  - Register the public webhook URL in the Square Developer Dashboard.
  - Use Square's webhook simulator or send test events.
- Example webhook curl (replace signature header with a valid value from Square or use a testing bypass in dev):
  - curl -X POST "https://your-ngrok.url/api/webhook" -H "Content-Type: application/json" -H "x-square-signature: <sig>" -d '@sample_event.json'

Security & operational guidance
- Secrets: use __User Secrets__ for local dev or environment variables / Key Vault in prod.
- Validate webhook signatures strictly.
- Log webhook incoming headers and parsed event IDs (avoid logging full sensitive payloads).
- If you must replay events, store `RawPayload` and `EventId` to prevent duplicate processing.
- Rate-limit and protect endpoints (auth for admin endpoints; webhooks only allowed with signature verification).

Troubleshooting
- 401/403 from Square on create payment:
  - Verify `Square:AccessToken`, `LocationId`, and `BaseUrl`.
- "Order not found" in `SquareCheckoutService`:
  - Ensure the `Order.OrderId` passed exists and includes `OrderItems` with valid `Combination` references.
- Webhook appears but not processed:
  - Confirm signature verification algorithm and header name match Square docs.
  - Confirm Square is sending to the exact URL registered (schema, host, path).
- Use the application logs (console/Debug/Azure App Diagnostics) to capture detailed failure messages.

References (where to edit)
- Checkout link creation: `Controllers/CheckoutController.cs`
- Payment link implementation: `Services/SquareCheckoutService.cs`
- Webhook verification & processing: `Services/SquareWebhookService.cs`
- Webhook endpoint: `Controllers/WebhookController.cs`

Useful Links
- Square Developer site - https://developer.squareup.com/us/en
- For understanding the object used to create payment links - https://developer.squareup.com/explorer/square/checkout-api/create-payment-link
- CheckoutAPI - https://developer.squareup.com/reference/square/checkout-api
- OrdersAPI (We didn't create orders directly but could be useful) - https://developer.squareup.com/reference/square/orders-api 
- Order Object - https://developer.squareup.com/reference/square/objects/Order

Challenges
- Copilot was apparently unable to make use of the square nuget package so we used HttpClient for making API calls.
- Mapping orders in square to local orders is not working currently.
    - Copilot led us to believe that we could include a reference id when composing the payload for payment link creation but this was wrong. It seems that to include a reference id we would need to create orders using the Orders API, we ran out of time to implement this. Ideally the reference id would be the local order id.
- There wasn't a centrally hosted application for testing webhooks so at first we tried polling square for payment updates but this is not recommended and we had trouble mapping orders, so we published the app in azure and set up a webhook and it works. however, becuase order mapping still isn't solved, the updates aren't handled.
- Toward the end of the quarter we discovered ngrok is a common tool for testing webhooks locally and will probably be useful in future development.