namespace FinanceTrackerServer.Models.Entities
{
    public enum TransactionType
    {
        Income,
        Expense
    }

    public class Transaction
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public TransactionType Type { get; set; }

        public int UserId { get; set; }
        public int? GroupId { get; set; }

        // Навигационные свойства
        public User User { get; set; }
        public Group? Group { get; set; }
    }
}
