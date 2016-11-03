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
        private Mock<IAuthorizationEnforcement> _authorizationEnforcementMock;

        [SetUp]
        public void Setup()
        {
            _authorizationEnforcementMock = new Mock<IAuthorizationEnforcement>();
        }

        [Test]
        public void AddClaimRuleWillAddARuleThatWillAlwaysFailWhenClaimIsMissing()
        {
            new ClaimRule<FakePermission, FakeClaim>((claim, permission) => true)
                .Evaluate(Enumerable.Empty<Claim>(), _authorizationEnforcementMock.Object, new FakePermission())
                .Should().BeFalse();
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void AddClaimRuleWillAddARuleThatWillPassIfClaimPassesTheTest(bool expectedOutcome)
        {
            new ClaimRule<FakePermission, FakeClaim>((claim, permission) => expectedOutcome)
                .Evaluate(new[] { new FakeClaim() },_authorizationEnforcementMock.Object, new FakePermission())
                .Should().Be(expectedOutcome);
        }

        [Test]
        public void AddClaimRuleWillEvaluateTheRuleForAllClaims()
        {
            var mock = new Mock<Func<FakeClaim, FakePermission, bool>>();
            var claim1 = new FakeClaim();
            var claim2 = new FakeClaim();
            var fakePermission = new FakePermission();

            new ClaimRule<FakePermission, FakeClaim>(mock.Object)
                .Evaluate(new[] { claim1, claim2 }, _authorizationEnforcementMock.Object, fakePermission);

            // verify that both claims have been passed to the predicate
            mock.Verify(func => func(claim1, fakePermission));
            mock.Verify(func => func(claim2, fakePermission));
        }
    }
}