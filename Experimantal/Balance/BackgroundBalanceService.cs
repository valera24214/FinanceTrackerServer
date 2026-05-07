namespace Experimantal.Balance
{
    public class BackgroundBalanceService:BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public BackgroundBalanceService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        private TimeSpan GetDelay()
        {
            var targetTime = DateTime.Today.AddDays(1);
            var delay = targetTime - DateTime.Now;

            return delay;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var balanceService = scope.ServiceProvider.GetService<IBalanceService>();
                await balanceService.CatchUpAllUsersBalances();
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                var delay = GetDelay();
                await Task.Delay(delay, stoppingToken);

                if (!stoppingToken.IsCancellationRequested)
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var balanceService = scope.ServiceProvider.GetService<IBalanceService>();
                        await balanceService.CatchUpAllUsersBalances();
                    }
                }
            }
        }

        
    }
}
