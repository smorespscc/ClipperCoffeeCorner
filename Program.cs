using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using ClipperCoffeeCorner.Data;
using ClipperCoffeeCorner.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------------------
// MVC controllers + views
// -----------------------------------------
builder.Services.AddControllersWithViews();

// -----------------------------------------
// EF Core DbContext
// -----------------------------------------
builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (builder.Environment.IsEnvironment("Testing"))
    {
        // No real DB, just in-memory
        options.UseInMemoryDatabase("ClipperCoffeeCornerTest");
    }
    else
    {
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"));
    }
});

// -----------------------------------------
// Square payment integration
// -----------------------------------------

// Prefer BaseUrl from configuration (appsettings.Development.json),
// fallback to the standard sandbox URL if not set.
var squareBaseUrl = builder.Configuration["Square:BaseUrl"];
if (string.IsNullOrWhiteSpace(squareBaseUrl))
{
    squareBaseUrl = "https://connect.squareupsandbox.com";
}

builder.Services.AddHttpClient("Square", client =>
{
    client.BaseAddress = new Uri(squareBaseUrl);
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
});

// Our checkout service that uses the Square HttpClient
builder.Services.AddScoped<ISquareCheckoutService, SquareCheckoutService>();

// -----------------------------------------
// Session (optional, but safe to keep)
// -----------------------------------------
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// -----------------------------------------
// HTTP request pipeline
// -----------------------------------------
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

app.UseSession();

app.UseAuthorization();

app.MapControllers();

// Map standard MVC routes for your HomeController / views
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
