namespace FinanceTrackerServer.Models.DTO.Stats
{
    public class StatsPeriodRequest
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public StatsPeriod? Period { get; set; }
        public enum StatsPeriod
        {
            Last7Days,
            Last30Days,
            CurrentMonth,
            LastMonth,
            CurrentYear,
        }
    }
}
