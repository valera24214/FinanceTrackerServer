using System.Transactions;

namespace FinanceTrackerServer.Models.Entities
{
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string InviteCode { get; set; } // Код для присоединения к группе

        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
