﻿using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Starcounter.Authorization.Authentication;
using Starcounter.Authorization.Core;
using Starcounter.Authorization.Middleware;
using Starcounter.Authorization.PageSecurity;
using Starcounter.Authorization.SignIn;
using Starcounter.Authorization.Tests.TestModel;
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
        public void AddStarcounterAuthorizationWithUserConfigures_AllServices()
        {
            _serviceCollection
                .AddStarcounterAuthorization<TestSettings, ScUserAuthenticationTicket, TicketToSession, User>();

            ExpectStartupFilter<AuthenticationStartupFilter>();
            ExpectMiddleware<SecurityMiddleware<ScUserAuthenticationTicket, User>>();
            GetRequiredService<IAuthorizationEnforcement>();
            GetRequiredService<ICurrentUserProvider<User>>();
            GetRequiredService<IAuthenticationBackend>().Should()
                .BeOfType<UserAuthenticationBackend<ScUserAuthenticationTicket, User>>();
        }

        [Test]
        public void AddTicketMaintenanceConfigures_StartupFilters()
        {
            _serviceCollection
                .AddAuthorizationTicketMaintenance<TestSettings, ScUserAuthenticationTicket, TicketToSession, User>();

            ExpectStartupFilter<TicketCreationStartupFilter>();
            ExpectStartupFilter<CleanupStartupFilter<ScUserAuthenticationTicket>>();
        }

        [Test]
        public void AddSecurityMiddlewareConfigures_SecurityMiddleware()
        {
            _serviceCollection
                .AddAuthorization()
                .AddSingleton(Mock.Of<IAuthenticationBackend>())
                .AddSecurityMiddleware<ScUserAuthenticationTicket, TicketToSession, User>();

            ExpectMiddleware<SecurityMiddleware<ScUserAuthenticationTicket, User>>();
        }

        private void ExpectMiddleware<T>()
        {
            var middlewares = GetRequiredService<IEnumerable<IPageMiddleware>>();
            middlewares
                .OfType<T>()
                .Should()
                .NotBeEmpty();
        }

        [Test]
        public void AddSecurityMiddlewareConfigures_AuthenticationUriProvider()
        {
            _serviceCollection.AddSecurityMiddleware<ScUserAuthenticationTicket, TicketToSession, User>();
            GetRequiredService<IAuthenticationUriProvider>();
        }

        [Test]
        public void AddSignInManagerConfigures_SignInManager()
        {
            _serviceCollection.AddSignInManager<ScUserAuthenticationTicket, TicketToSession, User>();
            GetRequiredService<ISignInManager<User>>();
        }

        [Test]
        public void AddUserConfigurationConfigures_CurrentUserProvider()
        {
            _serviceCollection.AddCurrentUserProvider<ScUserAuthenticationTicket, TicketToSession, User>();
            GetRequiredService<ICurrentUserProvider<User>>();
        }

        [Test]
        public void AddClaimTypeConfigures_StartupFilter()
        {
            _serviceCollection.AddClaimType<ClaimTemplate>("claimType");
            ExpectStartupFilter<ClaimCreatorStartupFilter<ClaimTemplate>>();
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
