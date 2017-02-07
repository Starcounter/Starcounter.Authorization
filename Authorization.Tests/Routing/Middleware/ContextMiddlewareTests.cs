using FluentAssertions;
using Moq;
using NUnit.Framework;
using Starcounter.Authorization.Routing;
using Starcounter.Authorization.Routing.Middleware;
using Starcounter.Authorization.Tests.PageSecurity;
using Starcounter.Authorization.Tests.Routing.Middleware.ExamplePages;

namespace Starcounter.Authorization.Tests.Routing.Middleware
{
    public class ContextMiddlewareTests
    {
        private ContextMiddleware _contextMiddleware;
        private Mock<ContextMiddleware.IObjectRetriever> _objectRetriever;

        [SetUp]
        public void Setup()
        {
            _objectRetriever = new Mock<ContextMiddleware.IObjectRetriever>();
            _contextMiddleware = new ContextMiddleware(_objectRetriever.Object);
        }

        [Test]
        public void ShouldUseExplicitUriToContextWhenAvailable()
        {
            var arguments = new[] {"a", "b"};
            var routingInfo = new RoutingInfo
            {
                Arguments = arguments,
                SelectedPageType = typeof(CustomUriToContextPage)
            };

            _contextMiddleware.Run(routingInfo, () => new Response());

            routingInfo.Context.Should().Be(arguments);
        }

        [Test]
        public void ShouldUseDbWhenPageContextIsExplicitButNoUriToContext()
        {
            var arguments = new[] {"a"};
            var routingInfo = new RoutingInfo
            {
                Arguments = arguments,
                SelectedPageType = typeof(ContextPage)
            };
            var thing = new Thing();
            _objectRetriever
                .Setup(retriever => retriever.GetById(typeof(Thing), "a"))
                .Returns(thing);

            _contextMiddleware.Run(routingInfo, () => new Response());

            routingInfo.Context.Should().Be(thing);
        }

        [Test]
        public void ShouldUseDbWhenPageIsIBoundButNoUriToContext()
        {
            var arguments = new[] {"a"};
            var routingInfo = new RoutingInfo
            {
                Arguments = arguments,
                SelectedPageType = typeof(BoundPage)
            };
            var thing = new Thing();
            _objectRetriever
                .Setup(retriever => retriever.GetById(typeof(Thing), "a"))
                .Returns(thing);

            _contextMiddleware.Run(routingInfo, () => new Response());

            routingInfo.Context.Should().Be(thing);
        }
    }
}