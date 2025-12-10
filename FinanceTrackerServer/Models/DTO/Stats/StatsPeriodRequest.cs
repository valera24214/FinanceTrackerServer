namespace FinanceTrackerServer.Models.DTO.Stats
{
    public class StatsPeriodRequest
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public StatsPeriod? Period { get; set; }
        
    }

    public enum StatsPeriod
    {
        Last7Days = 0,
        Last30Days = 1,
        CurrentMonth = 2,
        LastMonth = 3,
        CurrentYear = 4,
    }
}
