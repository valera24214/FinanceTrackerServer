namespace FinanceTrackerServer.Models.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public CategoryType Type { get; set; }


        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }

    public enum CategoryType
    {
        Expense,  
        Income   
    }
}
