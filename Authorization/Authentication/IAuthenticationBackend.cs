using System.Collections.Generic;
using System.Security.Claims;
using Starcounter.Authorization.Core;

namespace Starcounter.Authorization.Authentication
{
    public interface IAuthenticationBackend
    {
        ClaimsPrincipal GetCurrentPrincipal();
    }
}