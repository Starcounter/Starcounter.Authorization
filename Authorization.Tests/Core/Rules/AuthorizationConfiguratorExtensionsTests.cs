using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Starcounter.Authorization.Core;
using Starcounter.Authorization.Core.Rules;

namespace Starcounter.Authorization.Tests.Core.Rules
{
    public class AuthorizationConfiguratorExtensionsTests
    {
        private AuthorizationRules _authorizationRules;

        [SetUp]
        public void Setup()
        {
            _authorizationRules = new AuthorizationRules();
        }

        [Test]
        public void RequireClaimShouldAddARuleThatWillAlwaysPassWhenClaimIsPresent()
        {
            _authorizationRules.RequireClaim<FakePermission, FakeClaim>();

            _authorizationRules.Get<FakePermission>()
                .Should()
                .ContainSingle(rule => rule(new []{new FakeClaim()}, new FakePermission()));
        }

        [Test]
        public void RequireClaimShouldAddARuleThatWillAlwaysFailWhenClaimIsMissing()
        {
            _authorizationRules.RequireClaim<FakePermission, FakeClaim>();

            _authorizationRules.Get<FakePermission>()
                .Should()
                .ContainSingle(rule => !rule(Enumerable.Empty<Claim>(), new FakePermission()));
        }

        [Test]
        public void AddClaimRuleWillAddARuleThatWillAlwaysFailWhenClaimIsMissing()
        {
            _authorizationRules.AddClaimRule<FakePermission, FakeClaim>((claim, permission) => true);

            _authorizationRules.Get<FakePermission>()
                .Should()
                .ContainSingle(rule => !rule(Enumerable.Empty<Claim>(), new FakePermission()));
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void AddClaimRuleWillAddARuleThatWillPassIfClaimPassesTheTest(bool expectedOutcome)
        {
            _authorizationRules.AddClaimRule<FakePermission, FakeClaim>((claim, permission) => expectedOutcome);

            _authorizationRules.Get<FakePermission>()
                .Should()
                .ContainSingle(rule => rule(new [] {new FakeClaim() }, new FakePermission()) == expectedOutcome);
        }

    }
}