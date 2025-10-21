namespace FinanceTrackerServer.Models.DTO.Users
{
    public class TelegramDto
    {
        public long Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string PhotoUrl { get; set; } = string.Empty;
        public long AuthDate { get; set; }
        public string Hash { get; set; } = string.Empty;
    }

    public class BindEmailDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
