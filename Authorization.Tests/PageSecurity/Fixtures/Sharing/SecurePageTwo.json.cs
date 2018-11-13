using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace Starcounter.Authorization.Tests.PageSecurity.Fixtures.Sharing
{
    [Authorize(Roles = Roles.ThingEditor)]
    public partial class SecurePageTwo : Json
    {
        static SecurePageTwo()
        {
            DefaultTemplate.Common.InstanceType = typeof(CommonPart);
        }
    }
}
