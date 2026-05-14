// DKS.Gateway — ASP.NET Core 8 + Ocelot
// Tạo project mới: dotnet new web -n DKS.Gateway
// Cài package:     dotnet add package Ocelot

using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Đọc ocelot.json
builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

// Đăng ký Ocelot
builder.Services.AddOcelot(builder.Configuration);

// CORS — cho phép ASP.NET MVC frontend gọi vào gateway
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

app.UseCors("AllowAll");

// Health check đơn giản
app.MapGet("/gateway/health", () => new
{
    status = "ok",
    service = "DKS-Gateway",
    routes = new[] { "/api/ai/*", "/api/booking/*", "/api/auth/*" }
});

// Ocelot xử lý tất cả route còn lại
await app.UseOcelot();

app.Run();
