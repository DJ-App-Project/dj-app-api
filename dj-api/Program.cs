using dj_api.Authentication;
using dj_api.Data;
using dj_api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Remove the API Key authentication code, as you're focusing on JWT only

#region auth (JWT Only)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "test", // The issuer of the JWT token
            ValidAudience = "test2", // The expected audience of the JWT token
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("f5be22a679e35ba82f04d1427dbe56b8fc7301e529a1322110715467da59e7ce")) // Your secret key (ideally store in appsettings.json)
        };
    });
builder.Services.AddHttpContextAccessor(); // Needed for any future use cases of HttpContext

#endregion

#region rateLimit
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});
builder.Services.AddRateLimiter(rateLimiterOptions =>
{
    rateLimiterOptions.AddSlidingWindowLimiter("sliding", options =>
    {
        options.PermitLimit = 100; // Number of requests in a specific time
        options.Window = TimeSpan.FromSeconds(20); // Specific time window
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 0;
    });
});
#endregion

// Add services to the container
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddSingleton<EventRepository>();
builder.Services.AddSingleton<MusicDataRepository>();
builder.Services.AddSingleton<UserRepository>();
builder.Services.AddSingleton<GuestUserRepository>();
builder.Services.AddSingleton<SongRepository>();

builder.Services.AddSingleton<TokenService>();

builder.Services.AddControllers();

// Swagger configuration for JWT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // JWT Authorization in Swagger UI
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter your JWT token in the format: Bearer {your-token-here}"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();  
app.UseAuthorization();  

app.MapControllers();

app.Run();
