using System.Security.Claims;
using api.Data;
using api.Interfaces;
using Api.IntegrationTests.Mocks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace Api.IntegrationTests.Infrastructure
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        public MockFileStorageService FileStorageMock { get; private set; } = null!;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Test");

            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                var fileStorageDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IFileStorageService));
                if (fileStorageDescriptor != null)
                    services.Remove(fileStorageDescriptor);

                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    var configuration = services.BuildServiceProvider()
                                                .GetRequiredService<IConfiguration>();
                    var conn = configuration.GetConnectionString("DefaultConnection");
                    options.UseNpgsql(conn, npgsqlOptions => npgsqlOptions.UseNetTopologySuite());
                });

                FileStorageMock = new MockFileStorageService();

                services.AddSingleton<IFileStorageService>(FileStorageMock);

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                try
                {
                    db.Database.EnsureCreated();
                }
                catch (Exception ex)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<CustomWebApplicationFactory>>();
                    logger.LogError(ex, "An error occurred creating the test database.");
                    throw;
                }

                services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            context.Principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
                        {
                        new Claim(ClaimTypes.Name, "TestUser"),
                        new Claim(ClaimTypes.NameIdentifier, "1010101010"),
                        new Claim("telegram_id", "1010101010"),
                        }, "Test"));

                            context.Success();
                            return Task.CompletedTask;
                        }
                    };
                });
            });

            Environment.SetEnvironmentVariable("TELEGRAM_BOT_TOKEN", "FAKE_TOKEN_FOR_TESTING");
            Environment.SetEnvironmentVariable("JWT__Issuer", "TestIssuer");
            Environment.SetEnvironmentVariable("JWT__Audience", "TestAudience");
            Environment.SetEnvironmentVariable("JWT__Secret", "SuperSecretTestKeyThatIsLongEnoughForHS256");
        }
    }
}