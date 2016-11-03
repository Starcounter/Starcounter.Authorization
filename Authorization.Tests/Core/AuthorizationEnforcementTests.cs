using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Starcounter.Authorization.Authentication;
using Starcounter.Authorization.Core;
using Starcounter.Authorization.Core.Rules;

namespace Starcounter.Authorization.Tests.Core
{
    public class AuthorizationEnforcementTests
    {
        private AuthorizationEnforcement _authorizationEnforcement;
        private Mock<IAuthorizationRulesSource> _rulesMock;
        private Mock<IAuthenticationBackend> _authenticationMock;
        private List<IAuthorizationRule<FakePermission>> _rules;
        private List<Claim> _claims;

        [SetUp]
        public void SetUp()
        {
            _rulesMock = new Mock<IAuthorizationRulesSource>();
            _authenticationMock = new Mock<IAuthenticationBackend>();
            _authorizationEnforcement = new AuthorizationEnforcement(_rulesMock.Object, _authenticationMock.Object);

            _rulesMock.Setup(source => source.Get<FakePermission>()).Returns(() => _rules);
            _authenticationMock.Setup(backend => backend.GetCurrentClaims()).Returns(() => _claims);
            _rules = new List<IAuthorizationRule<FakePermission>>();
            _claims = new List<Claim>();
        }

        [Test]
        public void ShouldRejectWhenThereAreNoRulesForPermission()
        {
            _authorizationEnforcement.CheckPermission(new FakePermission()).Should().BeFalse();
        }

        [Test]
        public void ShouldGrantWhenThereIsARuleAndClaimsPassTheTest()
        {
            var ruleMock = new Mock<IAuthorizationRule<FakePermission>>();
            ruleMock.Setup(rule => rule.Evaluate(It.IsAny<IEnumerable<Claim>>(), It.IsAny<IAuthorizationEnforcement>(), It.IsAny<FakePermission>()))
                .Returns(true);
            _rules.Add(ruleMock.Object);
            var checkedPermission = new FakePermission();

            _authorizationEnforcement.CheckPermission(checkedPermission).Should().BeTrue();
            ruleMock.Verify(rule => rule.Evaluate(_claims, _authorizationEnforcement, checkedPermission));
        }
    }
}