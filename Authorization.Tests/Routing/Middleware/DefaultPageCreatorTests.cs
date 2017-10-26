using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Starcounter.Authorization.Routing;
using Starcounter.Authorization.Tests.PageSecurity;
using Starcounter.Authorization.Tests.Routing.Middleware.ExamplePages;

namespace Starcounter.Authorization.Tests.Routing.Middleware
{
    public class DefaultPageCreatorTests
    {
        [Test]
        public void ShouldCallInit()
        {
            var response = Router.DefaultPageCreator(new RoutingInfo() {SelectedPageType = typeof(InitPage)});

            response.Resource.As<InitPage>().HasBeenInitialized.Should().BeTrue();
        }

        [Test]
        public void ShouldCallHandleContextSupport()
        {
            var context = new Thing();
            var response = Router.DefaultPageCreator(
                    new RoutingInfo()
                    {
                        SelectedPageType = typeof(ContextPage),
                        Context = context
                    });

            response.Resource.As<ContextPage>().Context.Should().BeSameAs(context);
        }
    }
}
