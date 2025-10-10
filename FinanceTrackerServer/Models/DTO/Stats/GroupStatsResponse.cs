using FinanceTrackerServer.Services;

namespace FinanceTrackerServer.Models.DTO.Stats
{
    public class GroupStatsResponse
    {
        public TransactionStats GroupTotal { get; set; } = new();
        public List<UserStats> UsersStats { get; set; } = new();
        public int ActiveUsersCount { get; set; }
    }
}
