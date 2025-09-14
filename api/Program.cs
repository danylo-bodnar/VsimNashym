using api.Data;
using api.Services;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using DotNetEnv;
using api.Interfaces;
using api.Repositories;

Env.Load(); // load Telegram__BotToken

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
    options.UseNpgsql(connectionString));

// Telegram
var botToken = builder.Configuration["TELEGRAM_BOT_TOKEN"];
if (string.IsNullOrWhiteSpace(botToken))
{
    throw new InvalidOperationException("Telegram Bot Token is missing in configuration (TELEGRAM_BOT_TOKEN).");
}


//Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Services
builder.Services.AddSingleton<ITelegramBotClient>(_ =>
    new TelegramBotClient(botToken));
builder.Services.AddScoped<BotHandler>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<IBotConversationService, BotConversationService>();


// Controllers
builder.Services.AddControllers();

// App
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

