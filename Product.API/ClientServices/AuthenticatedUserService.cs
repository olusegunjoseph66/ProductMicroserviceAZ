using Product.Application.Interfaces.Services;
using System.Security.Claims;

namespace Product.API.ClientServices
{
#pragma warning disable CS8601 // Possible null reference assignment.
    public class AuthenticatedUserService : IAuthenticatedUserService
    {
        public AuthenticatedUserService(IHttpContextAccessor httpContextAccessor)
        {
            _ = int.TryParse(httpContextAccessor.HttpContext?.User?.FindFirstValue("userId"), out int userKey);
            var role = httpContextAccessor.HttpContext?.User?.FindFirstValue("userRole");
            
            UserId = userKey;
            Role = role;
        }

        public int UserId { get; set; }
        public string Role { get; set; }
    }
#pragma warning restore CS8601 // Possible null reference assignment.
}
