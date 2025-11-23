using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using ClipperCoffeeCorner.Data;
using ClipperCoffeeCorner.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// MVC controllers + views
builder.Services.AddControllersWithViews();

// EF Core DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Square payment integration ---

// Base URL for Square – read from config, fall back to sandbox host
var squareBaseUrl = builder.Configuration["Square:BaseUrl"]
                    ?? "https://connect.squareupsandbox.com";

// HttpClient for Square API
builder.Services.AddHttpClient("Square", client =>
{
    client.BaseAddress = new Uri(squareBaseUrl);
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));

    // Optional default Square-Version header
    var apiVersion = builder.Configuration["Square:ApiVersion"];
    if (!string.IsNullOrWhiteSpace(apiVersion))
    {
        client.DefaultRequestHeaders.Add("Square-Version", apiVersion);
    }
});

// Our checkout service that uses the Square HttpClient
builder.Services.AddScoped<ISquareCheckoutService, SquareCheckoutService>();

// --- Session (optional, but safe to keep) ---
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

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

app.UseSession();

app.UseAuthorization();

// Attribute-routed API controllers: /api/...
app.MapControllers();

// MVC default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

