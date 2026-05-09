using Microsoft.EntityFrameworkCore;
using OrderService.Models;
using Stripe;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);
// Add CORS policy configurable for dev and cloud
var allowedOrigins = builder.Configuration["Cors:AllowedOrigins"] ?? Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS") ?? "http://localhost:4200";
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

// Azure Key Vault integration
var keyVaultEnabled = builder.Configuration.GetValue<bool>("KeyVault:Enabled");
if (keyVaultEnabled)
{
    var vaultUri = builder.Configuration["KeyVault:VaultUri"];
    builder.Configuration.AddAzureKeyVault(new Uri(vaultUri), new DefaultAzureCredential());
}

StripeConfiguration.ApiKey = builder.Configuration["Stripe:ApiKey"];

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer();

builder.Services.AddSingleton<OrderService.Services.OrderEventPublisher>();

var app = builder.Build();



app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
// Use CORS before authentication/authorization
app.UseCors("AllowAngularDev");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
