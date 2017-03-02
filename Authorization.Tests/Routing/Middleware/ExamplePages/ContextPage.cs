using Starcounter.Authorization.Routing;
using Starcounter.Authorization.Tests.PageSecurity;

namespace Starcounter.Authorization.Tests.Routing.Middleware.ExamplePages
{
    public class ContextPage : IPageContext<Thing>
    {
        public Thing Context { get; set; }

        public void HandleContext(Thing context)
        {
            Context = context;
        }
    }
}