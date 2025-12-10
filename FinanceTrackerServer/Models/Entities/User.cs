using System.Text.RegularExpressions;
using System.Transactions;

namespace FinanceTrackerServer.Models.Entities
{
    public class User
    {
        public int Id { get; set; }

        public int? GroupId { get; set; }
        public Group? Group { get; set; }

        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<AuthAccount> AuthAccounts { get; set; } = new List<AuthAccount>();
    }
}
