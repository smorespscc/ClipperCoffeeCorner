using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc;
using ClipperCoffeeCorner.Dtos.Menu;
using ClipperCoffeeCorner.Dtos.Orders;
using ClipperCoffeeCorner.Dtos.Ui;
using ClipperCoffeeCorner.Dtos.Queue;
using ClipperCoffeeCorner.Dtos.Auth;


namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MapControllers : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new { Message = "MapControllers is working!" });
        }
    }
}

// Move the top-level statements into a Main method
public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // MVC (views) + API controllers
        builder.Services.AddControllersWithViews();

        // Swagger
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // (Optional) CORS for your web app front-end during local dev
        const string CorsPolicy = "AllowLocal";
        builder.Services.AddCors(opt =>
        {
            opt.AddPolicy(CorsPolicy, p => p
                .WithOrigins(
                    "http://localhost:5173", // Vite
                    "http://localhost:3000", // CRA/Next
                    "http://localhost:4200"  // Angular
                )
                .AllowAnyHeader()
                .AllowAnyMethod());
        });

        var app = builder.Build();

        // ----- Pipeline -----
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }
        else
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseDefaultFiles(new DefaultFilesOptions
        {
            DefaultFileNames = new List<string> { "index.html" }
        });
        app.UseStaticFiles();

        app.UseRouting();

        // (Optional) CORS must be after UseRouting and before Map*
        app.UseCors(CorsPolicy);

        app.UseAuthorization();

        // ✅ Attribute-routed API endpoints (e.g., [Route("auth")])
        app.MapControllers();

        // ✅ Conventional MVC route for your views (Home/Index)
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}
