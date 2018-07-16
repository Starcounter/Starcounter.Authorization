using System.Security.Claims;

namespace Starcounter.Authorization.Authentication
{
    internal interface IAuthenticationBackend
    {
        ClaimsPrincipal GetCurrentPrincipal();
    }
}