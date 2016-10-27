using Simplified.Ring3;

namespace Starcounter.Authorization.Core
{
    public class SystemUserClaim : Claim
    {
        public SystemUser SystemUser { get; private set; }

        public SystemUserClaim(SystemUser systemUser)
        {
            SystemUser = systemUser;
        }
    }
}