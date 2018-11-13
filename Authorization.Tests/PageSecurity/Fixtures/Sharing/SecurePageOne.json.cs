using Microsoft.AspNetCore.Authorization;
using Starcounter.Authorization.Tests.TestModel;

namespace Starcounter.Authorization.Tests.PageSecurity.Fixtures.Sharing
{
    [Authorize(Roles = Roles.ThingViewer)]
    public partial class SecurePageOne : Json
    {
        static SecurePageOne()
        {
            DefaultTemplate.Common.InstanceType = typeof(CommonPart);
        }

    }
}
