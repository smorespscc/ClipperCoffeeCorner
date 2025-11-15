using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.Extensions.Options; 
using SendGrid;
using Twilio;
using WaitTimeTesting.Data;
using WaitTimeTesting.Options;
using WaitTimeTesting.Services;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<WaitTimeNotificationService>();  // service
builder.Services.AddLogging(config => config.AddConsole());  // For ILogger
builder.Services.AddSingleton<IWaitTimeEstimator, WaitTimeEstimator>();
builder.Services.AddSingleton<INotificationService, TwilioNotificationService>();
builder.Services.AddSingleton<INotificationService, SendGridNotificationService>();
builder.Services.AddSingleton<IOrderStorage, MockOrderStorage>();

// FOR TESTING:
builder.Services.AddScoped<IOrderRepository, InMemoryOrderRepository>();

// FOR PRODUCTION (I hope):
// builder.Services.AddScoped<IOrderRepository, DbOrderRepository>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=waittime.db"));
//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
