// Program.cs  (.NET 7/8 minimal hosting)

using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// ---------- Services ----------
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        // System.Text.Json is camelCase by default; keeping it explicit is fine.
        o.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        o.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.Configure<ApiBehaviorOptions>(o =>
{
    // Let [ApiController] return 400 automatically when model validation fails
    // (default behavior; this line is optional)
    o.SuppressModelStateInvalidFilter = false;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Allow local web apps to call the API during dev
const string CorsPolicy = "AllowLocal";
builder.Services.AddCors(opt =>
{
    opt.AddPolicy(CorsPolicy, p => p
        .WithOrigins(
            "http://localhost:5173", // Vite
            "http://localhost:3000", // CRA/Next.js
            "http://localhost:4200"  // Angular
        )
        .AllowAnyHeader()
        .AllowAnyMethod());
});

// (Optional) Force ports if you want consistent URLs:
builder.WebHost.ConfigureKestrel(options =>
{
    // HTTP 5000 and HTTPS 7001
    options.ListenLocalhost(5000);
    options.ListenLocalhost(7001, o => o.UseHttps());
});

var app = builder.Build();

// ---------- Pipeline ----------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors(CorsPolicy);

// If/when you add auth:
// app.UseAuthentication();
// app.UseAuthorization();

app.MapControllers();

app.Run();
