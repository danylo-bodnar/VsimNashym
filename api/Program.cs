using api.Data;
using api.Services;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using DotNetEnv;
using api.Interfaces;
using api.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using api.Options;
using FluentValidation;
using api.Exceptions;
using System.Security.Claims;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "5015";
builder.WebHost.UseUrls($"http://*:{port}");

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Exceptions
builder.Services.AddProblemDetails(configure =>
{
    configure.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);
    };
});
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Validation
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly, includeInternalTypes: true);

// DB
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "DefaultConnection string is missing or empty. " +
        "Check appsettings.json, appsettings.Development.json, or .env file. " +
        $"Current value: '{connectionString ?? "null"}'");
}
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        connectionString,
        npgsqlOptions => npgsqlOptions.UseNetTopologySuite()));

// Telegram
var botToken = builder.Configuration["TELEGRAM_BOT_TOKEN"];
if (string.IsNullOrWhiteSpace(botToken))
{
    throw new InvalidOperationException("Telegram Bot Token is missing in configuration (TELEGRAM_BOT_TOKEN).");
}


//Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IConnectionRepository, ConnectionRepository>();

// Services
builder.Services.AddSingleton<ITelegramBotClient>(_ =>
    new TelegramBotClient(botToken));
builder.Services.AddScoped<BotHandler>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddSingleton<IBotMessenger, BotMessenger>();
builder.Services.AddScoped<IConnectionService, ConnectionService>();
builder.Services.AddSingleton<IFileStorageService>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();

    var supabaseUrl = configuration["Supabase:Url"];
    var supabaseApiKey = configuration["Supabase:ApiKey"];
    var bucketName = configuration["Supabase:BucketName"];
    var logger = provider.GetRequiredService<ILogger<SupabaseFileStorageService>>();

    if (string.IsNullOrWhiteSpace(supabaseUrl))
        throw new InvalidOperationException("Supabase URL is not configured.");

    if (string.IsNullOrWhiteSpace(supabaseApiKey))
        throw new InvalidOperationException("Supabase API key is not configured.");

    if (string.IsNullOrWhiteSpace(bucketName))
        throw new InvalidOperationException("Supabase bucket name is not configured.");

    return new SupabaseFileStorageService(supabaseUrl, supabaseApiKey, bucketName, logger);
});

builder.Services.AddHostedService<LocationCleanupService>();



builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SameTelegramUser", policy =>
        policy.RequireAssertion(ctx =>
        {
            var httpContext = ctx.Resource as HttpContext;

            if (httpContext == null)
                return false;

            var routeTelegramId =
                httpContext.Request.RouteValues["telegramId"]?.ToString();

            var claimTelegramId =
                ctx.User.FindFirstValue("telegram_id");

            return routeTelegramId == claimTelegramId;
        }));
});





// Controllers 
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.NumberHandling =
            System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals;
        options.JsonSerializerOptions.ReferenceHandler =
                    System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

// Add JwtOptions
builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection("JWT")
);

// Configure JWT authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
    options.DefaultChallengeScheme =
    options.DefaultForbidScheme =
    options.DefaultScheme =
    options.DefaultSignInScheme =
    options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;

}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],

        ValidateAudience = true,
        ValidAudience = builder.Configuration["JWT:Audience"],

        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
        System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"]!)
    )
    };
});

// Cors
var frontendUrls = new[]
{
    "https://vsim-nashym.vercel.app",
    "https://vsim-nashym-qa8ybhrt0-danyios-projects.vercel.app",
    "http://localhost:5173"
};


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(frontendUrls)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// App
var app = builder.Build();

// Auto-run migrations here
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (db.Database.IsRelational())
        {
            db.Database.Migrate();
        }
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
        throw;
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
