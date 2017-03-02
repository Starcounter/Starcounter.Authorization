using System;
using System.Linq.Expressions;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Starcounter.Authorization.Core;

namespace Starcounter.Authorization.Tests.PageSecurity
{
    public class PageSecurityCheckClassTests
    {
        private Mock<IAuthorizationEnforcement> _authEnforcementMock;
        private Authorization.PageSecurity.PageSecurity _pageSecurity;

        [SetUp]
        public void Setup()
        {
            _authEnforcementMock = new Mock<IAuthorizationEnforcement>();
            _pageSecurity = new Authorization.PageSecurity.PageSecurity(
                _authEnforcementMock.Object,
                Authorization.PageSecurity.PageSecurity.CreateThrowingDeniedHandler<UnauthorizedException>());
        }

        [Test]
        public void CheckClassAsksForProperPermission_RequirePermission()
        {
            _pageSecurity.CheckClass(typeof(ExamplePage), new object[0]);

            _authEnforcementMock.Verify(enforcement => enforcement.CheckPermission(It.IsAny<ViewThing>()));
        }

        [Test]
        public void CheckClassAsksForProperPermission_RequirePermissionData()
        {
            var thing = new Thing();

            _pageSecurity.CheckClass(typeof(ExampleDataPage), thing);

            _authEnforcementMock.Verify(enforcement => enforcement.CheckPermission(It.Is<ViewSpecificThing>(permission => permission.Thing == thing)));
        }

        [Test]
        public void CheckClassReturnsFalseWhenPassedNull_RequirePermissionData()
        {
            _pageSecurity.CheckClass(typeof(ExampleDataPage), new Thing())
                .Should().BeFalse();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void CheckClassReturnsResultOfTheCheck_RequirePermissionData(bool expectedOutcome)
        {
            _authEnforcementMock.Setup(enforcement => enforcement.CheckPermission(It.IsAny<ViewSpecificThing>()))
                .Returns(expectedOutcome);

            _pageSecurity.CheckClass(typeof(ExampleDataPage), new Thing())
                .Should().Be(expectedOutcome);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void CheckClassReturnsResultOfTheCheck_RequirePermission(bool expectedOutcome)
        {
            _authEnforcementMock.Setup(enforcement => enforcement.CheckPermission(It.IsAny<ViewThing>()))
                .Returns(expectedOutcome);

            _pageSecurity.CheckClass(typeof(ExamplePage), new object[0])
                .Should().Be(expectedOutcome);
        }

        [Test]
        public void CheckClassUsesCustomCheck()
        {
            var permission = new ViewThing();
            _authEnforcementMock.Setup(enforcement => enforcement.CheckPermission(permission))
                .Returns(true)
                .Verifiable();

            _pageSecurity.CheckClass(typeof(ExampleCustomPage), permission);

            _authEnforcementMock.Verify();
        }
    }
}