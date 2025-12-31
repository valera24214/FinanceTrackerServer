namespace FinanceTrackerServer.Models.Entities
{
    public class Balance
    {
        public int Id { get; set; }
        public int UserId {  get; set; }
        public decimal UserBalance { get; set; }
        public DateTime Date {  get; set; }

        public User User { get; set; }
    }
}
