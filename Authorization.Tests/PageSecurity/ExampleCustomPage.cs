using System.Reflection.Emit;
using Starcounter.Authorization.Attributes;
using Starcounter.Authorization.Core;
using Starcounter.Authorization.Routing;

namespace Starcounter.Authorization.Tests.PageSecurity
{
    public class ExampleCustomPage : Json, IPageContext<ViewThing>
    {
        public Permission Permission { get; set; }

        [CustomCheckClass]
        public static ViewThing CreatePermission(ViewThing args)
        {
            return args;
        }

        [UriToContext]
        public static ViewThing CreateContext(string[] args)
        {
            return new ViewThing();
        }

        public void HandleContext(ViewThing context)
        {
            
        }
    }
}