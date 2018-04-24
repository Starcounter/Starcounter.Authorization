using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Starcounter.Authorization.Authentication;
using Starcounter.Authorization.Core;
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
        public void AddSecurityMiddlewareConfigures_SecurityMiddleware()
        {
            _serviceCollection
                .AddAuthorization()
                .AddSingleton(Mock.Of<IAuthenticationBackend>())
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
        public void AddSignInManagerConfigures_SignInManager()
        {
            _serviceCollection.AddSignInManager<ScUserAuthenticationTicket, User>();
            GetRequiredService<ISignInManager<ScUserAuthenticationTicket, User>>();
        }

        [Test]
        public void AddAuthenticationConfigures_AuthorizationEnforcement_ButRequiresAddAuthorization()
        {
            _serviceCollection.AddAuthentication<ScUserAuthenticationTicket>()
                .AddAuthorization();
            GetRequiredService<IAuthorizationEnforcement>();
        }

        [Test]
        public void AddUserConfigurationConfigures_CurrentUserProvider()
        {
            _serviceCollection.AddUserConfiguration<ScUserAuthenticationTicket, User>();
            GetRequiredService<ICurrentUserProvider<User>>();
        }

        [Test]
        public void AddFakeAuthenticationConfigures_AuthorizationEnforcement()
        {
            _serviceCollection.AddFakeAuthentication()
                .AddAuthorization();
            GetRequiredService<IAuthorizationEnforcement>();
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
