using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Starcounter.Authorization.Core;
using Starcounter.Authorization.Core.Rules;

namespace Starcounter.Authorization.Tests.Core.Rules
{
    public class AuthorizationRulesTests
    {
        private AuthorizationRules _authorizationRules;

        [SetUp]
        public void Setup()
        {
            _authorizationRules = new AuthorizationRules();
        }

        [Test]
        public void GetReturnsEmptyWhenNoRulesHaveBeenSetUp()
        {
            _authorizationRules.Get<FakePermission>()
                .Should().BeEmpty();
        }
        
        [Test]
        public void GetReturnsAllRulesForGivenPermission()
        {
            IAuthorizationRule<FakePermission> rule1 = new PredicateRule<FakePermission>((claims, enforcement, perm) => true);
            IAuthorizationRule<FakePermission> rule2 = new PredicateRule<FakePermission>((claims, enforcement, perm) => false);
            _authorizationRules.AddRule(rule1);
            _authorizationRules.AddRule(rule2);

            _authorizationRules.Get<FakePermission>()
                .Should()
                .BeEquivalentTo(rule1, rule2);
        }
    }
}
