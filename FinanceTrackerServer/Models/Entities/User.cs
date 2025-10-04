using System.Text.RegularExpressions;
using System.Transactions;

namespace FinanceTrackerServer.Models.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }

        // Навигационные свойства
        public int? GroupId { get; set; }
        public Group Group { get; set; }
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
