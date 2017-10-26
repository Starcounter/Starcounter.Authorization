using Starcounter.Authorization.Routing;

namespace Starcounter.Authorization.Tests.Routing.Middleware.ExamplePages
{
    public class InitPage : Json, IInitPage
    {
        public bool HasBeenInitialized { get; set; }

        public void Init()
        {
            HasBeenInitialized = true;
        }
    }
}