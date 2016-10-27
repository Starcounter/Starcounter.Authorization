using Simplified.Ring2;
using Simplified.Ring3;

namespace Starcounter.Authorization.Core
{
    public class Authentication
    {
        public static Person GetCurrentPerson()
        {
            return SystemUser.GetCurrentSystemUser()?.WhoIs as Person;
        }
    }
}