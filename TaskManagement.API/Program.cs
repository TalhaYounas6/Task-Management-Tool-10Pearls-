using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TaskManagement.API.Data;
using TaskManagement.API.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container (This enables your Controllers / Express-style routes)
builder.Services.AddControllers();

// 1. Add Database connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Add Identity
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// 3. Add JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        // The "!" tells C# we promise this key exists in appsettings.json
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

// Swagger (Provides a UI to test your APIs later, similar to Postman)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// THIS is the line that builds the app
var app = builder.Build();

// Configure the HTTP request pipeline (Your Middleware)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// SECURITY MIDDLEWARE: Order matters here! Auth MUST come before Controllers
app.UseAuthentication();
app.UseAuthorization();

// Tells the app to map your routes based on your Controllers
app.MapControllers();

// Starts the server!
app.Run();