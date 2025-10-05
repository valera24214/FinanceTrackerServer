namespace FinanceTrackerServer.Models.DTO.Transactions
{
    public class UpdateTransactionDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
    }
}
