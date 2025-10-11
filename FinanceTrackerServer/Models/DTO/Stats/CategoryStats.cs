using FinanceTrackerServer.Models.Entities;

namespace FinanceTrackerServer.Models.DTO.Stats
{
    public class CategoryStats
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public CategoryType Type { get; set; }
        public decimal TotalAmount { get; set; }
        public int TransactionCount { get; set; }
        public decimal Percentage { get; set; } 
    }
}
