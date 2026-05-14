// DKS.Gateway — Ocelot + JWT forward
// dotnet add package Ocelot
// dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer

using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(builder.Environment.ContentRootPath)
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

// ── JWT (dùng cùng Key/Issuer/Audience với auth-service) ─────────────────────
var jwtSettings = builder.Configuration.GetSection("Jwt");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"]!))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(o => o.AddPolicy("AllowAll", p =>
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

// KHÔNG dùng UseHttpsRedirection ở gateway — chạy HTTP nội bộ
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/gateway/health", () => new
{
    status = "ok",
    routes = new[] { "/api/ai/*", "/api/booking/*", "/api/auth/*" }
});

await app.UseOcelot();
app.Run();

//// DKS.Gateway — ASP.NET Core 8 + Ocelot
//// Tạo project mới: dotnet new web -n DKS.Gateway
//// Cài package:     dotnet add package Ocelot

//using Ocelot.DependencyInjection;
//using Ocelot.Middleware;

//var builder = WebApplication.CreateBuilder(args);

//// Đọc ocelot.json
//builder.Configuration
//    .SetBasePath(builder.Environment.ContentRootPath)
//    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
//    .AddEnvironmentVariables();

//// Đăng ký Ocelot
//builder.Services.AddOcelot(builder.Configuration);

//// CORS — cho phép ASP.NET MVC frontend gọi vào gateway
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAll", policy =>
//        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
//});

//var app = builder.Build();

//app.UseCors("AllowAll");

//// Health check đơn giản
//app.MapGet("/gateway/health", () => new
//{
//    status = "ok",
//    service = "DKS-Gateway",
//    routes = new[] { "/api/ai/*", "/api/booking/*", "/api/auth/*" }
//});

//// Ocelot xử lý tất cả route còn lại
//await app.UseOcelot();

//app.Run();
