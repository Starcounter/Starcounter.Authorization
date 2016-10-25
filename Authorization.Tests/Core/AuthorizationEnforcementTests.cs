using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Starcounter.Authorization.Core;

namespace Starcounter.Authorization.Tests.Core
{
    public class AuthorizationEnforcementTests
    {
        private AuthorizationEnforcement _authorizationEnforcement;
        private Mock<IAuthorizationRulesSource> _rulesMock;
        private Mock<IAuthenticationBackend> _authenticationMock;
        private readonly List<Func<IEnumerable<Claim>, FakePermission, bool>> _rules = new List<Func<IEnumerable<Claim>, FakePermission, bool>>();
        private readonly List<Claim> _claims = new List<Claim>();

        [SetUp]
        public void SetUp()
        {
            _rulesMock = new Mock<IAuthorizationRulesSource>();
            _authenticationMock = new Mock<IAuthenticationBackend>();
            _authorizationEnforcement = new AuthorizationEnforcement(_rulesMock.Object, _authenticationMock.Object);

            _rulesMock.Setup(source => source.Get<FakePermission>()).Returns(() => _rules);
            _authenticationMock.Setup(backend => backend.GetCurrentClaims()).Returns(() => _claims);
        }

        [Test]
        public void ShouldRejectWhenThereAreNoRulesForPermission()
        {
            _authorizationEnforcement.TryPermission(new FakePermission()).Should().BeFalse();
        }

        [Test]
        public void ShouldGrantWhenThereIsARuleAndClaimsPassTheTest()
        {
            var mock = new Mock<Func<IEnumerable<Claim>, FakePermission, bool>>();
            mock.Setup(func => func(It.IsAny<IEnumerable<Claim>>(), It.IsAny<FakePermission>()))
                .Returns(true);
            _rules.Add(mock.Object);
            var checkedPermission = new FakePermission();

            _authorizationEnforcement.TryPermission(checkedPermission).Should().BeTrue();
            mock.Verify(func => func(_claims, checkedPermission));
        }
    }
}