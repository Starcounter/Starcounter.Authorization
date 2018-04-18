using Starcounter.Authorization.Routing;
using Starcounter.Authorization.Tests.TestModel;

namespace Starcounter.Authorization.Tests.Routing.Middleware.ExamplePages
{
    public class BoundContextPageWithDifferentTypes : Json, IBound<Thing>, IPageContext<ThingItem>
    {
        public ThingItem Context { get; private set; }

        public void HandleContext(ThingItem context)
        {
            Context = context;
        }
    }
}