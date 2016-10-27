using System;
using System.Linq;
using FluentAssertions;
using Moq;
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

        [Test]
        public void AddClaimRuleWillEvaluateTheRuleForAllClaims()
        {
            var mock = new Mock<Func<FakeClaim, FakePermission, bool>>();
            _authorizationRules.AddClaimRule<FakePermission, FakeClaim>(mock.Object);
            var claim1 = new FakeClaim();
            var claim2 = new FakeClaim();
            var fakePermission = new FakePermission();

            _authorizationRules.Get<FakePermission>()
                .First().Invoke(new[] {claim1, claim2}, fakePermission);

            // verify that both claims have been passed to the predicate
            mock.Verify(func => func(claim1, fakePermission));
            mock.Verify(func => func(claim2, fakePermission));
        }
    }
}