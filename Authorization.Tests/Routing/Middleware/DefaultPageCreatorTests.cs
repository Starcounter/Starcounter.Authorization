using FluentAssertions;
using NUnit.Framework;
using Starcounter.Authorization.Routing;
using Starcounter.Authorization.Tests.Fixtures;
using Starcounter.Authorization.Tests.Routing.Middleware.ExamplePages;

namespace Starcounter.Authorization.Tests.Routing.Middleware
{
    public class DefaultPageCreatorTests
    {
        [Test]
        public void ShouldCallInit()
        {
            var defaultPageCreator = new DefaultPageCreator();
            var response = defaultPageCreator.Create(new RoutingInfo() {SelectedPageType = typeof(InitPage)});

            response.Resource.As<InitPage>().HasBeenInitialized.Should().BeTrue();
        }

        [Test]
        public void ShouldCallHandleContextSupport()
        {
            var defaultPageCreator = new DefaultPageCreator();
            var context = new Thing();
            var response = defaultPageCreator.Create(
                    new RoutingInfo()
                    {
                        SelectedPageType = typeof(ContextPage),
                        Context = context
                    });

            response.Resource.As<ContextPage>().Context.Should().BeSameAs(context);
        }
    }
}
