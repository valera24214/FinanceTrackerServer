namespace FinanceTrackerServer.Models.DTO.Users
{
    public class TelegramDto
    {
        public long Id { get; set; }
        public string Username { get; set; } = string.Empty;
    }

    public class BindEmailDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
