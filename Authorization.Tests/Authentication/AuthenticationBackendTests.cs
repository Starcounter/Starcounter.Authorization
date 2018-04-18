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
        private AuthenticationBackend<UserSession> _sut;
        private Mock<ICurrentSessionRetriever<UserSession>> _sessionRetrieverMock;
        private ClaimsPrincipal _returnedPrincipal;
        private Mock<IStringSerializer<ClaimsPrincipal>> _serializerMock;

        [SetUp]
        public void SetUp()
        {
            _sessionRetrieverMock = new Mock<ICurrentSessionRetriever<UserSession>>();
            _serializerMock = new Mock<IStringSerializer<ClaimsPrincipal>>();
            _sut = new AuthenticationBackend<UserSession>(
                _sessionRetrieverMock.Object,
                Mock.Of<ILogger<AuthenticationBackend<UserSession>>>(),
                _serializerMock.Object
            );
        }

        [Test]
        public void WhenCurrentSessionIsNullThenAnonymousPrincipalIsReturned()
        {
            _sessionRetrieverMock.Setup(retriever => retriever.GetCurrentSession())
                .Returns((UserSession) null);

            Excercise();

            _returnedPrincipal.Identities.Should().BeEmpty();
            _returnedPrincipal.Claims.Should().BeEmpty();
        }

        [Test]
        public void WhenCurrentSessionIsNotNullThenDeserializedPrincipalIsReturned()
        {
            var serializedPrincipal = "serializedPrincipal";
            var deserializedPrincipal = new ClaimsPrincipal();
            _sessionRetrieverMock.Setup(retriever => retriever.GetCurrentSession())
                .Returns(new UserSession()
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