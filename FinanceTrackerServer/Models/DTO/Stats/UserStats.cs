using FinanceTrackerServer.Services;

namespace FinanceTrackerServer.Models.DTO.Stats
{
    public class UserStats
    {
        public int UserId { get; set; }
        public TransactionStats Stats { get; set; } = new();
    }
}
