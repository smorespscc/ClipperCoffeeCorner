using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
