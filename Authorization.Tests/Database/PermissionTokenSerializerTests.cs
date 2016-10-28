using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Starcounter.Advanced;
using Starcounter.Authorization.Database;

namespace Starcounter.Authorization.Tests.Database
{
    public class PermissionTokenSerializerTests
    {
        [DatabaseAttribute]
        // GetObjectId uses IBindable to retrieve object's identity
        public class FakeDbObject : IBindable
        {
            public ulong Identity { get; set; }
            public IBindableRetriever Retriever { get; set; }
            public Guid Unimportant { get; set; }
        }

        public class ExampleWithDbObject
        {
            public FakeDbObject DbObject { get; set; }
            public Guid ImportantGuid { get; set; }
        }

        private PermissionTokenSerializer _serializer;

        [SetUp]
        public void Setup()
        {
            _serializer = new PermissionTokenSerializer();
        }

        [Test]
        public void ValuesReferringToTheSameDbObjectShouldBeTheSame()
        {
            const ulong identity = 1234;
            var serialized1 = _serializer.Serialize(CreateExampleWithDbOfSpecifiedIdentity(identity));
            var serialized2 = _serializer.Serialize(CreateExampleWithDbOfSpecifiedIdentity(identity));

            serialized1.Should().Be(serialized2);
        }

        [Test]
        public void ValuesReferringToDifferentDbObjectsShouldDiffer()
        {
            var serialized1 = _serializer.Serialize(CreateExampleWithDbOfSpecifiedIdentity(1));
            var serialized2 = _serializer.Serialize(CreateExampleWithDbOfSpecifiedIdentity(2));

            serialized1.Should().NotBe(serialized2);
        }

        [Test]
        public void ValuesReferringToTheSameDbObjectButWithDifferentValuesShouldDiffer()
        {
            ulong dbObjectIdentity = 1344;
            var serialized1 = _serializer.Serialize(CreateExampleWithDbOfSpecifiedIdentity(dbObjectIdentity, importantGuid: Guid.NewGuid()));
            var serialized2 = _serializer.Serialize(CreateExampleWithDbOfSpecifiedIdentity(dbObjectIdentity, importantGuid: Guid.NewGuid()));

            serialized1.Should().NotBe(serialized2);
        }

        private ExampleWithDbObject CreateExampleWithDbOfSpecifiedIdentity(ulong dbObjectIdentity, Guid importantGuid = default(Guid))
        {
            return new ExampleWithDbObject()
            {
                DbObject = new FakeDbObject {Identity = dbObjectIdentity, Unimportant = Guid.NewGuid()},
                ImportantGuid = importantGuid
            };
        }
    }
}