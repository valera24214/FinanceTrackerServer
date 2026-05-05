using System.Security.Claims;

namespace FinanceTrackerServer.Models
{
    public interface IUserProvider
    {
        int UserId { get; }
    }
    public class UserProvider : IUserProvider
    {
        private readonly IHttpContextAccessor _accesor;
        public UserProvider(IHttpContextAccessor accessor)
        {
            _accesor = accessor;
        }

        public int UserId
        {
            get
            {
                var claims = _accesor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
                return claims != null ? int.Parse(claims.Value) : 0;
            }
        }
    }
}
