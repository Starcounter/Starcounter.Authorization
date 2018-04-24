using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Starcounter.Authorization.Authentication;
using Starcounter.Authorization.Model.Serialization;
using Starcounter.Authorization.Tests.TestModel;

namespace Starcounter.Authorization.Tests.Authentication
{
    public class AuthenticationBackendTests
    {
        private AuthenticationBackend<ScUserAuthenticationTicket> _sut;
        private Mock<IAuthenticationTicketProvider<ScUserAuthenticationTicket>> _authenticationTicketProviderMock;
        private ClaimsPrincipal _returnedPrincipal;
        private Mock<IStringSerializer<ClaimsPrincipal>> _serializerMock;

        [SetUp]
        public void SetUp()
        {
            _authenticationTicketProviderMock = new Mock<IAuthenticationTicketProvider<ScUserAuthenticationTicket>>();
            _serializerMock = new Mock<IStringSerializer<ClaimsPrincipal>>();
            _sut = new AuthenticationBackend<ScUserAuthenticationTicket>(
                _authenticationTicketProviderMock.Object,
                _serializerMock.Object
            );
        }

        [Test]
        public void WhenAuthenticationTicketIsNullThenAnonymousPrincipalIsReturned()
        {
            _authenticationTicketProviderMock.Setup(provider => provider.GetCurrentAuthenticationTicket())
                .Returns((ScUserAuthenticationTicket) null);

            Excercise();

            _returnedPrincipal.Identities.Should().BeEmpty();
            _returnedPrincipal.Claims.Should().BeEmpty();
        }

        [Test]
        public void WhenAuthenticationTicketIsNotNullThenDeserializedPrincipalIsReturned()
        {
            var serializedPrincipal = "serializedPrincipal";
            var deserializedPrincipal = new ClaimsPrincipal();
            _authenticationTicketProviderMock.Setup(provider => provider.GetCurrentAuthenticationTicket())
                .Returns(new ScUserAuthenticationTicket()
                {
                    PrincipalSerialized = serializedPrincipal
                });
            _serializerMock.Setup(serializer => serializer.Deserialize(serializedPrincipal))
                .Returns(deserializedPrincipal);

            Excercise();

            _returnedPrincipal.Should().BeSameAs(deserializedPrincipal);
        }

        private void Excercise()
        {
            _returnedPrincipal = _sut.GetCurrentPrincipal();
        }
    }
}