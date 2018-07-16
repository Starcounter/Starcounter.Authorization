using System.Collections.Generic;
using System.Security.Claims;
using FluentAssertions;
using NUnit.Framework;
using Starcounter.Authorization.Model.Serialization;
using Starcounter.Authorization.Tests.TestModel;

namespace Starcounter.Authorization.Tests.Model.Serialization
{
    public class ClaimDbConverterTests
    {
        private ClaimDbConverter _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new ClaimDbConverter();
        }

        [Test]
        public void ScalarPropertiesArePersisted()
        {
            var type = "type";
            var value = "value";
            var valueType = "valueType";
            var issuer = "issuer";
            var originalIssuer = "originalIssuer";

            var claimDb = new ClaimTemplate();

            _sut.Pack(new Claim(type, value, valueType, issuer, originalIssuer), claimDb);
            var deserialized = _sut.Unpack(claimDb);

            deserialized.Should().BeEquivalentTo(new Claim(type, value, valueType, issuer, originalIssuer));
        }

        [Test]
        public void KeyValuePropertiesArePersisted()
        {
            var claim = new Claim("type", "value");
            var keyValuePair = new KeyValuePair<string, string>("key", "value");
            claim.Properties.Add(keyValuePair);
            var claimDb = new ClaimTemplate();

            _sut.Pack(claim, claimDb);
            var deserialized = _sut.Unpack(claimDb);

            deserialized.Properties.Should().Contain(keyValuePair);
        }
    }
}