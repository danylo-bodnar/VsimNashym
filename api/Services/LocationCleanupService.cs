using api.Interfaces;

namespace api.Services
{
    public class LocationCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public LocationCleanupService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var userRepository = scope.ServiceProvider
                    .GetRequiredService<IUserRepository>();

                var cutoff = DateTime.UtcNow.AddDays(-90);
                var inactiveUsers =
                    await userRepository.GetInactiveUsersOlderThanAsync(cutoff, stoppingToken);

                foreach (var user in inactiveUsers)
                    user.Location = null;

                await userRepository.SaveChangesAsync();

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}
