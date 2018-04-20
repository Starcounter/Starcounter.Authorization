using FluentAssertions;
using NUnit.Framework;
using Starcounter.Authorization.Model;

namespace Starcounter.Authorization.Tests.Model
{
    public class SqlEscapingTests
    {
        [TestCase("ident", "\"ident\"")]
        [TestCase("namespace.ident", "\"namespace\".\"ident\"")]
        public void IdentifierEscapeTests(string input, string expectedOutput)
        {
            SqlEscaping.EscapeSql(input).Should().Be(expectedOutput);
        }
    }
}