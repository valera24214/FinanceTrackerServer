using FinanceTrackerServer.Models.DTO.Users;

namespace FinanceTrackerServer.Models.DTO
{
    public class GroupDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<UserDto> Users { get; set; }
    }
}
