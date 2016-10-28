using System.Collections.Generic;
using Simplified.Ring2;
using Simplified.Ring3;
using Starcounter.Authorization.Core;

namespace Starcounter.Authorization.Authentication
{
    public class SystemUserAuthentication : IAuthenticationBackend
    {
        public IEnumerable<Claim> GetCurrentClaims()
        {
            var currentSystemUser = SystemUser.GetCurrentSystemUser();
            if (currentSystemUser != null)
            {
                yield return new SystemUserClaim(currentSystemUser);

                var currentPerson = currentSystemUser.WhoIs as Person;
                if (currentPerson != null)
                {
                    yield return new PersonClaim(currentPerson);
                }
            }
        }
    }
}