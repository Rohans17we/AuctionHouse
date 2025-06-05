using TheAuctionHouse.Domain.ServiceContracts;
using TheAuctionHouse.Domain.Services;
using TheAuctionHouse.Domain.DataContracts;
using TheAuctionHouse.Data.EFCore.InMemory;
using TheAuctionHouse.Common;
using TheAuctionHouse.WebAPI.Configuration;
using TheAuctionHouse.WebAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRouting(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
});

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Configure authorization and policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    
    // Fallback policy for protecting all endpoints by default
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// Configure JWT settings
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// Add JWT authentication
// Configure jwt authentication
var jwtSettingsSection = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettingsSection);
var jwtSettings = jwtSettingsSection.Get<JwtSettings>();

if (jwtSettings == null || string.IsNullOrEmpty(jwtSettings.SecretKey))
{
    throw new InvalidOperationException("JWT settings are not properly configured. Check appsettings.json and secrets.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            RequireExpirationTime = true,
            ClockSkew = TimeSpan.Zero, // Remove clock skew to enforce exact token expiration
            RoleClaimType = ClaimTypes.Role, // Explicitly set role claim type
            NameClaimType = JwtRegisteredClaimNames.Sub, // Map 'sub' claim to NameIdentifier
            ValidateTokenReplay = true
    };
    options.Events = new JwtBearerEvents
    {        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Append("Token-Expired", "true");
            }
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            // Log successful authentication
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Successfully authenticated token for user {User}", context.Principal?.Identity?.Name);
            return Task.CompletedTask;
        }
    };
});

// Register TokenService
builder.Services.AddScoped<ITokenService, TokenService>();

// Configure SQLite database
var databaseSettings = builder.Configuration.GetSection("DatabaseSettings").Get<DatabaseSettings>() 
    ?? new DatabaseSettings { Provider = "SQLite", ConnectionStrings = new() { SQLite = "Data Source=TheAuctionHouseDB.sqlite" } };

// Add SQLite DbContext
builder.Services.AddDbContext<SqliteDbContext>(options =>
    options.UseSqlite(databaseSettings.ConnectionStrings.SQLite));

Console.WriteLine($"Using SQLite database: {databaseSettings.ConnectionStrings.SQLite}");

// Register interfaces with SQLite implementations
builder.Services.AddScoped<IAppUnitOfWork>(provider =>
{
    var context = provider.GetRequiredService<SqliteDbContext>();
    return new SqliteUnitOfWork(context);
});

// Add OpenAPI/Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "TheAuctionHouse API", Version = "v1" });
});

// Register application services
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<IAuctionService, AuctionService>();
builder.Services.AddScoped<IBidService, BidService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IAdministratorService, AdministratorService>();
builder.Services.AddScoped<IPortalUserService, PortalUserService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Apply migrations and seed SQLite database
using (var scope = app.Services.CreateScope())
{
    try
    {
        var sqliteContext = scope.ServiceProvider.GetRequiredService<SqliteDbContext>();
        sqliteContext.Database.Migrate();
        Console.WriteLine("SQLite database migrations applied successfully.");
        DatabaseSeeder.SeedDatabase(sqliteContext);
        Console.WriteLine("SQLite database seeded successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error with SQLite database: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TheAuctionHouse API V1");
        c.RoutePrefix = "swagger";
    });
}

// The correct middleware order is important!
app.UseHttpsRedirection();
app.UseCors("AllowAll");  // CORS should be before authentication
app.UseAuthentication();   // Authentication should be before Authorization
app.UseAuthorization();    // Authorization depends on Authentication

// Map endpoints using top-level route registration
app.MapControllers();
app.MapGet("/", () => "TheAuctionHouse API");

app.Run();
