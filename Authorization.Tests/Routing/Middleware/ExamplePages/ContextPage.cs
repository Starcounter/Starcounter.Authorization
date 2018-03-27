using Starcounter.Authorization.Routing;
using Starcounter.Authorization.Tests.Fixtures;

namespace Starcounter.Authorization.Tests.Routing.Middleware.ExamplePages
{
    public class ContextPage : Json, IPageContext<Thing>
    {
        public Thing Context { get; set; }

        public void HandleContext(Thing context)
        {
            Context = context;
        }
    }
}