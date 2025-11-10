using FinanceTrackerServer.Models.DTO.Users;
using FinanceTrackerServer.Models.Entities;

namespace FinanceTrackerServer.Services
{
    public enum ClientType
    {
        MobileClient,
        TelegramClient
    }

    public class UserDtoMapper
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public UserDtoMapper(IHttpContextAccessor contextAccessor)
        { _contextAccessor = contextAccessor; }

        public UserDto MapToClientSpecificDto(User user)
        {
            var clientType = GetClientType();

            return clientType switch
            {
                ClientType.TelegramClient when !user.TelegramId.HasValue =>
                    throw new InvalidOperationException("User doesn't have telegram data"),

                ClientType.MobileClient when string.IsNullOrEmpty(user.Email) =>
                    throw new InvalidOperationException("User doesn't have mobile data"),

                ClientType.TelegramClient => new TelegramUserDto
                {
                    Id = user.Id,
                    Name = user.Username,
                    TelegramId = user.TelegramId!.Value,
                    TelegramUsername = user.TelegramUsername
                },

                ClientType.MobileClient => new MobileUserDto
                {
                    Id = user.Id,
                    Name = user.Username,
                    Email = user.Email,
                },

                _ => throw new NullReferenceException("No info about client type")
            };
        }

        private ClientType GetClientType()
        {
            var context = _contextAccessor.HttpContext;
            if (context == null)
                throw new InvalidOperationException("HttpContext is not available");

            if (context.Request.Headers.ContainsKey("X-Telegram-User-Id"))
                return ClientType.TelegramClient;

            var userAgent = context.Request.Headers["User-Agent"].ToString();
            if (userAgent.Contains("TelegramBot"))
                return ClientType.TelegramClient;

            return ClientType.MobileClient; 




        }
    }
}
