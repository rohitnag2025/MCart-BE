using Microsoft.EntityFrameworkCore;
using AdminService.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AdminDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer();

builder.Services.AddHttpClient("ProductService", c => c.BaseAddress = new Uri("http://productservice:8080"));
builder.Services.AddHttpClient("OrderService", c => c.BaseAddress = new Uri("http://orderservice:8080"));
builder.Services.AddHttpClient("UserService", c => c.BaseAddress = new Uri("http://userservice:8080"));

var app = builder.Build();



app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
