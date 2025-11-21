using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Restore default logging providers
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add Azure App Service log providers (REQUIRED for Log Stream)
builder.Logging.AddAzureWebAppDiagnostics();

// Optional filters
builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
builder.Logging.AddFilter("System", LogLevel.Warning);
builder.Logging.AddFilter("Controllers.WebhookController", LogLevel.Information);

// -------------------------------------------

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add distributed in-memory cache and session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    // Session cookie settings
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // allow session cookie even if consent policies are used
});

// Configure named HttpClient for Square and register the SquareCheckoutService
var squareBaseUrl = builder.Configuration["Square:BaseUrl"] ?? (builder.Configuration["Square:Environment"] == "Production"
    ? "https://connect.squareup.com"
    : "https://connect.squareupsandbox.com");

builder.Services.AddHttpClient("Square", client =>
{
    client.BaseAddress = new Uri(squareBaseUrl);
    // Square API version header can be set per-request; optional default:
    var apiVersion = builder.Configuration["Square:ApiVersion"];
    if (!string.IsNullOrEmpty(apiVersion))
    {
        client.DefaultRequestHeaders.Add("Square-Version", apiVersion);
    }
});

builder.Services.AddScoped<Services.ISquareCheckoutService, Services.SquareCheckoutService>();

// Register webhook verification service so WebhookController can be constructed
builder.Services.AddScoped<Services.ISquareWebhookService, Services.SquareWebhookService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseDefaultFiles(new DefaultFilesOptions
{
    DefaultFileNames = new List<string> { "index.html" }
});

app.UseStaticFiles();

app.UseRouting();

// Add session middleware before authorization / endpoint execution
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
