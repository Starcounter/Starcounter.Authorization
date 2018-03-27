using FluentAssertions;
using NUnit.Framework;
using Starcounter.Authorization.Routing;
using Starcounter.Authorization.Tests.Fixtures;
using Starcounter.Authorization.Tests.PageSecurity;
using Starcounter.Authorization.Tests.Routing.Middleware.ExamplePages;

namespace Starcounter.Authorization.Tests.Routing.Middleware
{
    public class PageContextSupportHandleContextTests
    {
        [Test]
        public void ShouldHandleContextExplicitlyWhenItsPossible()
        {
            var context = new Thing();
            var page = new ContextPage();

            PageContextSupport.HandleContext(page, context);

            page.Context.Should().Be(context);
        }

        [Test]
        public void ShouldSetDataWhenIBound()
        {
            var context = new Thing();
            var page = new BoundPage();

            PageContextSupport.HandleContext(page, context);

            page.Data.Should().Be(context);
        }

        [Test]
        public void ShouldHandleContextExplicitlyWhenIBoundAndIPageContext()
        {
            var context = new Thing();
            var page = new BoundContextPage();

            PageContextSupport.HandleContext(page, context);

            page.Context.Should().Be(context);
            page.Data.Should().BeNull();
        }

        [Test]
        public void ShouldNotBrakeWhenIBoundAndIPageContextAreDifferent()
        {
            var context = new ThingItem();
            var page = new BoundContextPageWithDifferentTypes();

            PageContextSupport.HandleContext(page, context);

            page.Context.Should().Be(context);
        }
    }
}