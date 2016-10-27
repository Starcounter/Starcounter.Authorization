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
            Func<IEnumerable<Claim>, FakePermission, bool> rule1 = (claims, permission) => true;
            Func<IEnumerable<Claim>, FakePermission, bool> rule2 = (claims, permission) => false;
            _authorizationRules.AddRule(rule1);
            _authorizationRules.AddRule(rule2);

            _authorizationRules.Get<FakePermission>()
                .Should()
                .BeEquivalentTo(rule1, rule2);
        }
    }
}
