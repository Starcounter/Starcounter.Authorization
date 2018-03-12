using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Moq;
using NUnit.Framework;
using Starcounter.Authorization.Authentication;
using Starcounter.Authorization.Core;

namespace Starcounter.Authorization.Tests.Core
{
    public class AuthorizationEnforcementTests
    {
        private AuthorizationEnforcement _authorizationEnforcement;
        private Mock<IAuthorizationService> _authorizationServiceMock;
        private Mock<IAuthenticationBackend> _authenticationMock;
        private ClaimsPrincipal _principal;

        [SetUp]
        public void SetUp()
        {
            _authorizationServiceMock = new Mock<IAuthorizationService>();
            _authenticationMock = new Mock<IAuthenticationBackend>();
            _authorizationEnforcement = new AuthorizationEnforcement(_authorizationServiceMock.Object, _authenticationMock.Object);
            _principal = new ClaimsPrincipal();
            _authenticationMock
                .Setup(authentication => authentication.GetCurrentPrincipal())
                .Returns(_principal);
        }

        [Test]
        public async Task ShouldExtractPrincipalFromAuthenticationAndPassItToAuthorizationService()
        {
            _authorizationServiceMock.Setup(service =>
                    service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(AuthorizationResult.Success());
            var resource = new object();
            var policyName = "policy";

            await _authorizationEnforcement.CheckPolicyAsync(policyName, resource);

            _authorizationServiceMock.Verify(service => service.AuthorizeAsync(_principal, resource, policyName));
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task ShouldReturnResultFromAuthorizationService(bool expectedResult)
        {
            _authorizationServiceMock.Setup(service =>
                    service.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), It.IsAny<string>()))
                .ReturnsAsync(expectedResult ? AuthorizationResult.Success() : AuthorizationResult.Failed());

            var result = await _authorizationEnforcement.CheckPolicyAsync("policy", null);

            result.Should().Be(expectedResult);
        }
    }
}