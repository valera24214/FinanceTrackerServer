using FinanceTrackerServer.Models.Entities;

namespace FinanceTrackerServer.Models.DTO
{
    public class GroupDto
    {
        public int Id { get; set; }
        public List<User> Users { get; set; }
    }
}
