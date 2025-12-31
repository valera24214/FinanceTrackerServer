namespace FinanceTrackerServer.Services.Interfaces
{
    public interface IBalanceService
    {
        Task<decimal> CalculateBalanceForPeriod(int userId, DateTime targetDate);
        Task CatchUpAllUsersBalances(CancellationToken stoppingToken = default, DateTime? startDate = null, int? userId = null);
    }
}
