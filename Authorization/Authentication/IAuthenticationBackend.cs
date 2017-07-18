using System.Collections.Generic;
using Starcounter.Authorization.Core;

namespace Starcounter.Authorization.Authentication
{
    public interface IAuthenticationBackend
    {
        IEnumerable<Claim> GetCurrentClaims();
    }
}