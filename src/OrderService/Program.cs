using Microsoft.EntityFrameworkCore;
using OrderService.Models;
using Stripe;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);
// Add CORS policy configurable for dev and cloud
var allowedOrigins = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS") ??
                     builder.Configuration["Cors:AllowedOrigins"] ??
                     builder.Configuration["FRONTEND_URL"] ??
                     "http://localhost:4200";
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev",
        policy => policy
            .WithOrigins(allowedOrigins.Split(';', StringSplitOptions.RemoveEmptyEntries))
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
    );
});


// Azure Key Vault integration (optional, keep as is)
var keyVaultEnabled = builder.Configuration.GetValue<bool>("KeyVault:Enabled");
if (keyVaultEnabled)
{
    var vaultUri = builder.Configuration["KeyVault:VaultUri"];
    builder.Configuration.AddAzureKeyVault(new Uri(vaultUri), new DefaultAzureCredential());
}

// Stripe API Key from env
StripeConfiguration.ApiKey = Environment.GetEnvironmentVariable("STRIPE_API_KEY") ?? builder.Configuration["Stripe:ApiKey"];

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Use SQL Server connection string from env
var sqlConnectionString = Environment.GetEnvironmentVariable("SQLSERVER_CONNECTION_STRING")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(sqlConnectionString));

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer();


builder.Services.AddSingleton<OrderService.Services.OrderEventPublisher>();
builder.Services.AddHostedService<OrderService.Services.PaymentEventConsumer>();
builder.Services.AddSingleton<OrderService.Services.InventoryEventPublisher>();

var app = builder.Build();

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
// Use CORS before authentication/authorization
app.UseCors("AllowAngularDev");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
