using System.Collections.Generic;
using System.Security.Claims;
using Starcounter.Authorization.Model;

namespace Starcounter.Authorization.SignIn
{
    internal interface IUserClaimsGatherer
    {
        IEnumerable<Claim> Gather(IUser user);
    }
}