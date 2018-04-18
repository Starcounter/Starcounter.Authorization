using System.Collections.Generic;
using System.Security.Claims;
using Starcounter.Authorization.Model;

namespace Starcounter.Authorization.SignIn
{
    public interface IUserClaimsGatherer
    {
        IEnumerable<Claim> Gather(IUserWithGroups user);
    }
}