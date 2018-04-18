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
        private ServiceCollection _serviceCollection;

        [SetUp]
        public void SetUpServiceCollection()
        {
            _serviceCollection = new ServiceCollection();
            AddDefaultServices(_serviceCollection);
        }

        [Test]
        public void AddSecurityMiddlewareTest()
        {
            _serviceCollection
                .AddAuthorization(auth => auth.AddPolicy("DoThings", builder => builder.RequireUserName("admin")))
                .AddSingleton(Mock.Of<IAuthenticationBackend>())
                // sc auth
                .AddSecurityMiddleware();

            var middlewares = GetRequiredService<IEnumerable<IPageMiddleware>>();
            middlewares
                .OfType<SecurityMiddleware>()
                .Should()
                .NotBeEmpty();
        }

        [Test]
        public void AddRouterTest()
        {
            _serviceCollection.AddRouter();
            GetRequiredService<Router>();
        }

        [Test]
        public void AddSignInManagerConfiguresAllDependencies()
        {
            _serviceCollection.AddSignInManager<UserSession, User>();
            GetRequiredService<ISignInManager<UserSession, User>>();
        }

        [Test]
        public void AddAuthenticationConfiguresAllDependencies()
        {
            _serviceCollection.AddAuthentication<UserSession>();
            GetRequiredService<IAuthenticationBackend>();
        }

        [Test]
        public void AddUserConfigurationConfiguresAllDependencies()
        {
            _serviceCollection.AddUserConfiguration<UserSession, User>();
            GetRequiredService<ICurrentUserProvider<User>>();
        }

        [Test]
        public void AddFakeAuthenticationConfiguresAllDependencies()
        {
            _serviceCollection.AddFakeAuthentication();
            GetRequiredService<IAuthenticationBackend>();
        }

        private void AddDefaultServices(IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddOptions()
                .AddLogging(builder => builder.ClearProviders());
        }

        private T GetRequiredService<T>()
        {
            return _serviceCollection.BuildServiceProvider().GetRequiredService<T>();
        }
    }
}
