using FluentAssertions;
using NUnit.Framework;
using Starcounter.Authorization.Authentication;

namespace Starcounter.Authorization.Tests.Authentication
{
    public class SecureRandomTests
    {
        private SecureRandom _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new SecureRandom();
        }

        [Test]
        public void GeneratesStringOfTwiceTheBytesLength()
        {
            var bytesLength = 12;
            var generatedString = _sut.GenerateRandomHexString(bytesLength);
            generatedString.Should().HaveLength(bytesLength*2);
        }

        [Test]
        public void GeneratesOnlyHexCharacters()
        {
            var generatedString = _sut.GenerateRandomHexString(12);
            generatedString.Should().MatchRegex("[0-9a-fA-F]+");
        }
    }
}