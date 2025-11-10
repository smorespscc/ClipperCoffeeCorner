using Microsoft.EntityFrameworkCore;
using Twilio;
using WaitTimeTesting.Data;
using WaitTimeTesting.Options;
using WaitTimeTesting.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<WaitTimeNotificationService>();  // service
builder.Services.AddLogging(config => config.AddConsole());  // For ILogger
builder.Services.AddSingleton<IWaitTimeEstimator, WaitTimeEstimator>();
builder.Services.AddSingleton<INotificationService, TwilioNotificationService>();
builder.Services.AddSingleton<IOrderStorage, MockOrderStorage>();

// FOR TESTING:
builder.Services.AddSingleton<IOrderQueue, InMemoryOrderQueue>();

// FOR PRODUCTION (I hope):
// builder.Services.AddScoped<IOrderQueue, DbOrderQueue>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Twilio configuration
builder.Services.Configure<TwilioSMSOptions>(
    builder.Configuration.GetSection("Twilio"));

// Initialize Twilio client globally
var twilioOptions = builder.Configuration.GetSection("Twilio").Get<TwilioSMSOptions>();
if (!string.IsNullOrEmpty(twilioOptions?.AccountSid) && !string.IsNullOrEmpty(twilioOptions?.AuthToken))
{
    TwilioClient.Init(twilioOptions.AccountSid, twilioOptions.AuthToken);
}

// Swagger for API testing
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Wait Time Notification API", Version = "v1" });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    // Enable Swagger UI in development
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Wait Time Notification API v1");
        c.RoutePrefix = "swagger";  // Go to /swagger to see UI
    });
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
