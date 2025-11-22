using ClipperCoffeeCorner.Data;
using ClipperCoffeeCorner.Options;
using ClipperCoffeeCorner.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore.SqlServer;
using SendGrid;
using System.Collections.Generic;
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

// Map attribute-routed API controllers: /api/...
app.MapControllers();

// Map standard MVC routes for your HomeController / views
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();