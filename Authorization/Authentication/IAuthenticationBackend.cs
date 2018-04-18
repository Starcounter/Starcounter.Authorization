using System.Security.Claims;

namespace Starcounter.Authorization.Authentication
{
    public interface IAuthenticationBackend
    {
        ClaimsPrincipal GetCurrentPrincipal();
    }
}