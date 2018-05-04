using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Starcounter.Authorization.Authentication;
using Starcounter.Authorization.ClaimManagement;
using Starcounter.Authorization.ClaimManagement.Central;
using Starcounter.Authorization.Core;
using Starcounter.Authorization.Middleware;
using Starcounter.Authorization.PageSecurity;
using Starcounter.Authorization.SignIn;
using Starcounter.Authorization.Tests.TestModel;
using Starcounter.Authorization.UserManagement;
using Starcounter.Authorization.UserManagement.Central;
using Starcounter.Startup.Abstractions;
using Starcounter.Startup.Routing;

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
        public void AddSecurityMiddlewareConfigures_AuthenticationStartupFilter()
        {
            _serviceCollection.AddSecurityMiddleware();
            ExpectStartupFilter<AuthenticationStartupFilter>();
        }

        [Test]
        public void AddSecurityMiddlewareConfigures_AuthenticationUriProvider()
        {
            _serviceCollection.AddSecurityMiddleware();
            GetRequiredService<IAuthenticationUriProvider>();
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
        public void AddClaimManagementConfigures_StartupFilter()
        {
            _serviceCollection.AddClaimManagement<ClaimDb>("claimType", typeof(object));
            ExpectStartupFilter<ClaimManagementStartupFilter<ClaimDb>>();
        }

        [Test]
        public void AddCentralClaimManagementConfigures_StartupFilter()
        {
            _serviceCollection.AddCentralClaimsManagement<ClaimDb>();
            ExpectStartupFilter<CentralClaimManagementStartupFilter<ClaimDb>>();
        }

        [Test]
        public void AddUserManagementConfigures_StartupFilter()
        {
            _serviceCollection.AddUserManagement<User>(typeof(object));
            ExpectStartupFilter<UserManagementStartupFilter<User>>();
        }

        [Test]
        public void AddCentralUserManagementConfigures_StartupFilter()
        {
            _serviceCollection.AddCentralUsersManagement<User>();
            ExpectStartupFilter<CentralUserManagementStartupFilter<User>>();
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

        private void ExpectStartupFilter<T>()
        {
            GetRequiredService<IEnumerable<IStartupFilter>>()
                .OfType<T>()
                .Should().NotBeEmpty();
        }
    }
}
