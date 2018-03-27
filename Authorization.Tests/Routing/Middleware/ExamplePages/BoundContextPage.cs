using Starcounter.Authorization.Routing;
using Starcounter.Authorization.Tests.Fixtures;

namespace Starcounter.Authorization.Tests.Routing.Middleware.ExamplePages
{
    public class BoundContextPage : Json, IBound<Thing>, IPageContext<Thing>
    {
        public Thing Context { get; set; }

        public void HandleContext(Thing context)
        {
            Context = context;
        }
    }
}