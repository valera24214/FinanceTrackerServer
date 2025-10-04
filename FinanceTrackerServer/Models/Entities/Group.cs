using System.Transactions;

namespace FinanceTrackerServer.Models.Entities
{
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
