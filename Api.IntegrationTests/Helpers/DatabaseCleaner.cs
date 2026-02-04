using api.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Api.IntegrationTests.Helpers
{
    public static class DatabaseCleaner
    {
        public static async Task CleanDatabase(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            context.Connections.RemoveRange(context.Connections);
            context.Users.RemoveRange(context.Users);

            await context.SaveChangesAsync();
        }
    }
}