using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Starcounter.Authorization.Authentication;
using Starcounter.Authorization.PageSecurity;
using Starcounter.Authorization.Routing;
using Starcounter.Authorization.Routing.Middleware;

namespace Starcounter.Authorization.Tests.PageSecurity
{
    public class ServicesCreationTests
    {
        [Test]
        public void AddAuthorizationTest()
        {
            var authenticationBackendMock = new Mock<IAuthenticationBackend>();
            var services = new ServiceCollection()
                // general
                .AddLogging()
                .AddOptions()
                // ms auth
                .AddAuthorization(auth => auth.AddPolicy("DoThings", builder => builder.RequireUserName("admin")))
                .AddSingleton(authenticationBackendMock.Object)
                // sc auth
                .AddSecurityMiddleware();

            var middlewares = services.BuildServiceProvider()
                .GetRequiredService<IEnumerable<IPageMiddleware>>();
            middlewares
                .OfType<SecurityMiddleware>()
                .Should()
                .NotBeEmpty();
        }

        [Test]
        public void AddRouterTest()
        {
            var services = new ServiceCollection()
                // general
                .AddRouter();

            services
                .BuildServiceProvider()
                .GetRequiredService<Router>();
        }


    }
}