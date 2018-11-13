using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace Starcounter.Authorization.Tests.PageSecurity.Fixtures.Sharing
{
    [Authorize(Roles = Roles.ThingEditor)]
    public partial class SecurePageWithCommonArray : Json
    {
        static SecurePageWithCommonArray()
        {
            DefaultTemplate.CommonArray.SetCustomGetElementType(arr => CommonPart.DefaultTemplate);
            DefaultTemplate.CommonObject.InstanceType = typeof(CommonPart);
        }
    }
}
