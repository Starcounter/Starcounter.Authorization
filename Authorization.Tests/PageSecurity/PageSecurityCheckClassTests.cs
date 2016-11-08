using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Starcounter.Authorization.Core;
using Starcounter.Internal;
using Starcounter.XSON;

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
            _pageSecurity = new Authorization.PageSecurity.PageSecurity(_authEnforcementMock.Object);
        }

        [Test]
        public void CheckClassAsksForProperPermission_RequirePermission()
        {
            _pageSecurity.CheckClass(typeof(ExamplePage), null);

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
            _pageSecurity.CheckClass(typeof(ExampleDataPage), null)
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

            _pageSecurity.CheckClass(typeof(ExamplePage), null)
                .Should().Be(expectedOutcome);
        }
    }

    public class PageSecurityEnhanceClassTests
    {
        private Mock<IAuthorizationEnforcement> _authEnforcementMock;
        private Authorization.PageSecurity.PageSecurity _pageSecurity;

        [SetUp]
        public void Setup()
        {
            _authEnforcementMock = new Mock<IAuthorizationEnforcement>();
            _pageSecurity = new Authorization.PageSecurity.PageSecurity(_authEnforcementMock.Object);
        }

        [Test]
        public void HandlerMarkedWithAttributeShouldAskForProperPermission_RequirePermission()
        {
            var jsonPatch = new JsonPatch();
            var page = CreatePage<ExamplePage>();
            _authEnforcementMock.Setup(enforcement => enforcement.CheckPermission(It.IsAny<ChangeThing>()))
                .Returns(true)
                .Verifiable();
            
//            page.Session = new Session();
//            jsonPatch.Apply(page, "{{\"op\":\"replace\",\"path\":\"/ChangeThing$\",\"value\":1}");
            page.Template.ChangeThing.ProcessInput(page, 1);
//            page.Template.ChangeThing.Setter(page, 1);

            _authEnforcementMock.Verify();
        }

        [Test]
        public void HandlerMarkedWithAttributeShouldThrowWhenPermissionIsRejected_RequirePermission()
        {
            var page = CreatePage<ExamplePage>();
            _authEnforcementMock.Setup(enforcement => enforcement.CheckPermission(It.IsAny<ChangeThing>()))
                .Returns(false);

            new Action(() => page.ChangeThing = 1).ShouldThrow<UnauthorizedException>();
        }

        [Test]
        public void HandlerWithNoAttributeShouldAskForProperPagePermission_RequirePermission()
        {
            var page = CreatePage<ExamplePage>();
            _authEnforcementMock.Setup(enforcement => enforcement.CheckPermission(It.IsAny<ViewThing>()))
                .Returns(true)
                .Verifiable();

            page.ActionNotMarked = 1;

            _authEnforcementMock.Verify();
        }

        [Test]
        public void HandlerWithNoAttributeShouldThrowWhenPagePermissionIsRejected_RequirePermission()
        {
            var page = CreatePage<ExamplePage>();
            _authEnforcementMock.Setup(enforcement => enforcement.CheckPermission(It.IsAny<ChangeThing>()))
                .Returns(false);

            new Action(() => page.ChangeThing = 1).ShouldThrow<UnauthorizedException>();
        }

        [Test]
        public void HandlerMarkedWithAttributeShouldAskForProperPermission_RequirePermissionData()
        {
            var thing = new Thing();
            var page = CreatePage<ExamplePage>();
            page.Data = thing;
            _authEnforcementMock.Setup(enforcement => enforcement.CheckPermission(It.Is<ViewSpecificThing>(perm => perm.Thing == thing)))
                .Returns(true)
                .Verifiable();

            page.ViewSpecificThing = 1;

            _authEnforcementMock.Verify();
        }

        [Test]
        public void HandlerMarkedWithAttributeShouldThrowWhenPermissionIsRejected_RequirePermissionData()
        {
            var thing = new Thing();
            var page = CreatePage<ExamplePage>();
            page.Data = thing;
            _authEnforcementMock.Setup(enforcement => enforcement.CheckPermission(It.Is<ViewSpecificThing>(perm => perm.Thing == thing)))
                .Returns(true);

            new Action(() => page.ViewSpecificThing = 1).ShouldThrow<UnauthorizedException>();
        }

        private T CreatePage<T>() where T : new()
        {
            _pageSecurity.EnhanceClass(typeof(T));
            return new T();
        }
    }
}