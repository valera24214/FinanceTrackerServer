namespace FinanceTrackerServer.Models.DTO.Users
{
    public class UserLoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class BindTelegramDto
    {
        public long TelegramUserId { get; set; }
        public string TelegramUsername { get; set; } = string.Empty;
    }
}
