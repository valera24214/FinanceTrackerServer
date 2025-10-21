namespace FinanceTrackerServer.Models.DTO.Users
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class MobileUserDto:UserDto
    {
        public string Email { get; set; }
    }

    public class TelegramUserDto : UserDto 
    {
        public long TelegramId { get; set; }
        public string TelegramUsername { get; set; }
    }
}
