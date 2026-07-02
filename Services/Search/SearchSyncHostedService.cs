using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SchoolProject.Services.Search
{
    public class SearchSyncHostedService : BackgroundService
    {
        private readonly ILogger<SearchSyncHostedService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public SearchSyncHostedService(ILogger<SearchSyncHostedService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Search sync heartbeat at: {time}", DateTimeOffset.Now);
                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}
