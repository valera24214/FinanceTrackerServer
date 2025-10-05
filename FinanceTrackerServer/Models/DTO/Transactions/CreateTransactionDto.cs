using FinanceTrackerServer.Models.Entities;

namespace FinanceTrackerServer.Models.DTO.Transactions
{
    public class CreateTransactionDto
    {
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public TransactionType Type { get; set; }
    }
}
