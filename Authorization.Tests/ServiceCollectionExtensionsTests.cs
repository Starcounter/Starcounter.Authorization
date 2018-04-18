using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Starcounter.Authorization.Authentication;
using Starcounter.Authorization.PageSecurity;
using Starcounter.Authorization.Routing;
using Starcounter.Authorization.Routing.Middleware;
using Starcounter.Authorization.SignIn;
using Starcounter.Authorization.Tests.TestModel;

namespace Starcounter.Authorization.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Test]
        public void AddSecurityMiddlewareTest()
        {
            var services = new ServiceCollection();
            AddDefaultServices(services);
            services
                .AddAuthorization(auth => auth.AddPolicy("DoThings", builder => builder.RequireUserName("admin")))
                .AddSingleton(Mock.Of<IAuthenticationBackend>())
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
            var services = new ServiceCollection();
            AddDefaultServices(services);

            services
                .AddRouter()
                .BuildServiceProvider()
                .GetRequiredService<Router>();
        }

        [Test]
        public void AddSignInManagerConfiguresAllDependencies()
        {
            var serviceCollection = new ServiceCollection();
            AddDefaultServices(serviceCollection);
            var signInManager = serviceCollection
                .AddSignInManager<UserSession, User>()
                .BuildServiceProvider()
                .GetRequiredService<ISignInManager<UserSession, User>>();

            signInManager.Should().NotBeNull();
        }

        private void AddDefaultServices(IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddOptions()
                .AddLogging(builder => builder.ClearProviders());
        }
    }
}
