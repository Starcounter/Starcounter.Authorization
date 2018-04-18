using System.Security.Claims;

namespace Starcounter.Authorization.Authentication
{
    public class AlwaysAdminAuthBackend : IAuthenticationBackend
    {
        public ClaimsPrincipal GetCurrentPrincipal()
        {
            return new ClaimsPrincipal(new ClaimsIdentity(new []{new Claim(ClaimsIdentity.DefaultNameClaimType, "admin"), }, "fake"));
        }
    }
}