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
        public void SerializedAndDeserializedGivesTheSameResult_Claim()
        {
            var sut = new Base64ClaimSerializer();
            var claim = new Claim("type", "value");

            var serialized = sut.Serialize(claim);
            var deserialized = sut.Deserialize(serialized);

            deserialized.Should().BeEquivalentTo(claim);
        }

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
        [Test]
        public void SerializedAndDeserializedGivesTheSameResult_ClaimsPrincipalDirect()
        {
            var claim = new Claim("type", "value");
            var authenticationType = "authentication";
            var principal = new ClaimsPrincipal(
                new ClaimsIdentity(new[] {claim}, authenticationType)
                );

            var memoryStream = new MemoryStream();
            using (var binaryWriter = new BinaryWriter(memoryStream, Encoding.UTF8, true))
            {
                principal.WriteTo(binaryWriter);
            }
            var deserialized = new ClaimsPrincipal(new BinaryReader(memoryStream, Encoding.UTF8));

            deserialized.Should().BeEquivalentTo(principal, options => options.IgnoringCyclicReferences());
        }
    }
}