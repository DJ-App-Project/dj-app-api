using dj_api.Authentication;
using dj_api.Data;
using dj_api.Repositories;
using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.OpenApi.Models;
var builder = WebApplication.CreateBuilder(args);

#region auth
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiKeyPolicy", policy =>

    {
        policy.Requirements.Add(new ApiKeyRequirement());
    });
});
builder.Services.AddScoped<IAuthorizationHandler, ApiKeyHandler>();
builder.Services.AddScoped<IApiKeyValidation, ApiKeyValidation>();//call apikeyValidation
builder.Services.AddHttpContextAccessor();//need for policy based auth
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
        options.PermitLimit = 100; // number of req in specific time
        options.Window = TimeSpan.FromSeconds(20);// specific time
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 0;
    });
});
#endregion

// Add services to the container.
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddSingleton<EventRepository>();
builder.Services.AddSingleton<MusicDataRepository>();
builder.Services.AddSingleton<UserRepository>();
builder.Services.AddSingleton<GuestUserRepository>();
builder.Services.AddSingleton<SongRepository>();


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen( c =>
{
    c.AddSecurityDefinition("X-API-Key", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        Name = "X-API-Key",
        In = ParameterLocation.Header,
        Description = "Enter your API key in the header using the 'ApiKey' format"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "X-API-Key"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
