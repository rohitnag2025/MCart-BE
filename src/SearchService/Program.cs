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

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
// Use CORS before authentication/authorization
app.UseCors("AllowAngularDev");
app.UseAuthorization();
app.MapControllers();
app.Run();
