using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Starcounter.Authorization.Core;
using Starcounter.Templates;

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
        public void HandlerMarkedWithAttribute_RequirePermission_ShouldAskForProperPermission()
        {
            var page = CreatePage<ExamplePage>();
            SetupPermissionToReturn<ChangeThing>(true);
            
            ChangePropertyInPage(page, p => p.Template.ChangeThing);

            _authEnforcementMock.Verify();
        }

        [Test]
        public void HandlerMarkedWithAttribute_RequirePermission_ShouldThrowWhenPermissionIsDenied()
        {
            var page = CreatePage<ExamplePage>();
            SetupPermissionToReturn<ChangeThing>(false);

            new Action(() => ChangePropertyInPage(page, p => p.Template.ChangeThing)).ShouldThrow<UnauthorizedException>();
        }

        [Test]
        public void HandlerMarkedWithAttribute_RequirePermission_ShouldWorkWhenPermissionIsGranted()
        {
            var page = CreatePage<ExamplePage>();
            SetupPermissionToReturn<ChangeThing>(true);

            ChangePropertyInPage(page, p => p.Template.ChangeThing);

            page.Changed.ShouldBeEquivalentTo(nameof(ExamplePage.ChangeThing));
        }

        [Test]
        public void HandlerWithNoAttribute_RequirePermissionOnPage_ShouldAskForProperPagePermission()
        {
            var page = CreatePage<ExamplePage>();
            SetupPermissionToReturn<ViewThing>(true);

            ChangePropertyInPage(page, p => p.Template.ActionNotMarked);

            _authEnforcementMock.Verify();
        }

        [Test]
        public void HandlerWithNoAttribute_RequirePermissionOnPage_ShouldThrowWhenPagePermissionIsDenied()
        {
            var page = CreatePage<ExamplePage>();
            SetupPermissionToReturn<ViewThing>(false);

            new Action(() => ChangePropertyInPage(page, p => p.Template.ActionNotMarked)).ShouldThrow<UnauthorizedException>();
        }

        [Test]
        public void HandlerWithNoAttribute_RequirePermissionOnPage_ShouldWorkWhenPermissionIsGranted()
        {
            var page = CreatePage<ExamplePage>();
            SetupPermissionToReturn<ViewThing>(true);

            ChangePropertyInPage(page, p => p.Template.ActionNotMarked);

            page.Changed.ShouldBeEquivalentTo(nameof(ExamplePage.ActionNotMarked));
        }

        [Test]
        public void HandlerMarkedWithAttribute_RequirePermissionData_ShouldAskForProperPermission()
        {
            var thing = new Thing();
            var page = CreatePage<ExamplePage>();
            page.Data = thing;
            _authEnforcementMock.Setup(enforcement => enforcement.CheckPermission(It.Is<ViewSpecificThing>(perm => perm.Thing == thing)))
                .Returns(true)
                .Verifiable();

            ChangePropertyInPage(page, p => p.Template.ViewSpecificThing);

            _authEnforcementMock.Verify();
        }

        [Test]
        public void HandlerMarkedWithAttribute_RequirePermissionData_ShouldThrowWhenPermissionIsDenied()
        {
            var thing = new Thing();
            var page = CreatePage<ExamplePage>();
            page.Data = thing;
            SetupPermissionToReturn<ViewSpecificThing>(false);

            new Action(() => ChangePropertyInPage(page, p => p.Template.ViewSpecificThing)).ShouldThrow<UnauthorizedException>();
        }

        [Test]
        public void HandlerMarkedWithAttribute_RequirePermissionData_ShouldWorkWhenPermissionIsGranted()
        {
            var thing = new Thing();
            var page = CreatePage<ExamplePage>();
            page.Data = thing;
            SetupPermissionToReturn<ViewSpecificThing>(true);

            ChangePropertyInPage(page, p => p.Template.ViewSpecificThing);

            page.Changed.ShouldBeEquivalentTo(nameof(ExamplePage.ViewSpecificThing));
        }

//        [Test]
//        public void SubpageHandlerWithNoAttributeShouldAskForProperPagePermission_RequirePermission()
//        {
//            var page = CreatePage<ExamplePage>().Elements.Add();
//            _authEnforcementMock.Setup(enforcement => enforcement.CheckPermission(It.IsAny<ViewThing>()))
//                .Returns(true)
//                .Verifiable();
//
//            ChangePropertyInPage(page, p => p.Template.ChangeSubThing);
//
//            _authEnforcementMock.Verify();
//        }
//
//        [Test]
//        public void SubpageHandlerWithNoAttributeShouldThrowWhenPagePermissionIsRejected_RequirePermission()
//        {
//            var page = CreatePage<ExamplePage>().Elements.Add();
//            _authEnforcementMock.Setup(enforcement => enforcement.CheckPermission(It.IsAny<ViewThing>()))
//                .Returns(false);
//
//            new Action(() => ChangePropertyInPage(page, p => p.Template.ChangeSubThing)).ShouldThrow<UnauthorizedException>();
//        }


        private void ChangePropertyInPage<T>(T page, Func<T, Property<long>> propertySelector) where T:Json
        {
            propertySelector(page).ProcessInput(page, 1);
        }

        private T CreatePage<T>() where T : new()
        {
            _pageSecurity.EnhanceClass(typeof(T));
            return new T();
        }

        private void SetupPermissionToReturn<T>(bool valueToReturn) where T : Permission
        {
            _authEnforcementMock.Setup(enforcement => enforcement.CheckPermission(It.IsAny<T>()))
                .Returns(valueToReturn)
                .Verifiable();
        }
    }
}