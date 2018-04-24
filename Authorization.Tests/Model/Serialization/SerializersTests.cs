using System.IO;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Starcounter.Authorization.Model.Serialization;

namespace Starcounter.Authorization.Tests.Model.Serialization
{
    public class SerializersTests
    {
        [Test]
        public void SerializedAndDeserializedGivesTheSameResult_ClaimsPrincipal()
        {
            var sut = new Base64ClaimsPrincipalSerializer();
            var claim = new Claim("type", "value");
            var authenticationType = "authentication";
            var principal = new ClaimsPrincipal(
                new ClaimsIdentity(new[] {claim}, authenticationType)
                );

            var serialized = sut.Serialize(principal);
            var deserialized = sut.Deserialize(serialized);

            deserialized.Should().BeEquivalentTo(principal, options => options.IgnoringCyclicReferences());
        }
    }
}