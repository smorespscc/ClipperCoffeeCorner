using ClipperCoffeeCorner.Data;
using ClipperCoffeeCorner.Options;
using ClipperCoffeeCorner.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.Extensions.Options;
using SendGrid;
using System.Collections.Generic;
using System.Net.Http.Headers;
using Twilio;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<INotificationService, TwilioNotificationService>();
builder.Services.AddSingleton<INotificationService, SendGridNotificationService>();
builder.Services.AddScoped<WaitTimeNotificationService>();
builder.Services.AddSingleton<IWaitTimeEstimator, WaitTimeEstimator>();

// Register EF Core DbContext with the connection string from configuration
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Twilio configuration (SMS notifications)
builder.Services.Configure<TwilioSMSOptions>(
    builder.Configuration.GetSection("Twilio"));

var twilioOptions = builder.Configuration.GetSection("Twilio").Get<TwilioSMSOptions>();
if (!string.IsNullOrEmpty(twilioOptions?.AccountSid) && !string.IsNullOrEmpty(twilioOptions?.AuthToken))
{
    TwilioClient.Init(twilioOptions.AccountSid, twilioOptions.AuthToken);
}

// SendGrid configuration (Email notifications)
builder.Services.Configure<SendGridOptions>(
    builder.Configuration.GetSection("SendGrid"));

builder.Services.AddSingleton<SendGridClient>(sp =>
{
    var sendGridOptions = sp.GetRequiredService<IOptions<SendGridOptions>>().Value;
    return new SendGridClient(sendGridOptions.ApiKey);
});

// --- Square payment integration ---

// Base URL for Square – read from config, fall back to sandbox host
var squareBaseUrl = builder.Configuration["Square:BaseUrl"]
                    ?? "https://connect.squareupsandbox.com/";

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

// Register EF Core DbContext with the connection string from configuration
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

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

app.UseAuthorization();

app.UseSession();

// Map attribute-routed API controllers: /api/...
app.MapControllers();

// Map standard MVC routes for your HomeController / views
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();