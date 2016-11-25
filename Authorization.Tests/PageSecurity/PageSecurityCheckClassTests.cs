using System;
using System.Linq.Expressions;
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
            _pageSecurity = new Authorization.PageSecurity.PageSecurity(_authEnforcementMock.Object, Authorization.PageSecurity.PageSecurity.CreateThrowingDeniedHandler<UnauthorizedException>());
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

            _pageSecurity.CheckClass(typeof(ExampleDataPage), new[] {thing});

            _authEnforcementMock.Verify(enforcement => enforcement.CheckPermission(It.Is<ViewSpecificThing>(permission => permission.Thing == thing)));
        }

        [Test]
        public void CheckClassReturnsFalseWhenPassedNull_RequirePermissionData()
        {
            _pageSecurity.CheckClass(typeof(ExampleDataPage), new object[0])
                .Should().BeFalse();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void CheckClassReturnsResultOfTheCheck_RequirePermissionData(bool expectedOutcome)
        {
            _authEnforcementMock.Setup(enforcement => enforcement.CheckPermission(It.IsAny<ViewSpecificThing>()))
                .Returns(expectedOutcome);

            _pageSecurity.CheckClass(typeof(ExampleDataPage), new[] {new Thing()})
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
    }

    public class PageSecurityEnhanceClassTests
    {
        private Mock<IAuthorizationEnforcement> _authEnforcementMock;
        private Authorization.PageSecurity.PageSecurity _pageSecurity;

        [SetUp]
        public void Setup()
        {
            _authEnforcementMock = new Mock<IAuthorizationEnforcement>();
            _pageSecurity = new Authorization.PageSecurity.PageSecurity(_authEnforcementMock.Object, Authorization.PageSecurity.PageSecurity.CreateThrowingDeniedHandler<UnauthorizedException>());
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

        [Test]
        public void PropertyWithoutHandler_RequirePermission_ShouldAskForProperPermission()
        {
            var page = CreatePage<ExamplePage>();
            _authEnforcementMock.Setup(enforcement => enforcement.CheckPermission(It.IsAny<ViewThing>()))
                .Returns(true)
                .Verifiable();

            ChangePropertyInPage(page, p => p.Template.PropertyOne);

            _authEnforcementMock.Verify();
        }

        [Test]
        public void PropertyWithoutHandler_RequirePermission_ShouldThrowWhenPermissionIsDenied()
        {
            var page = CreatePage<ExamplePage>();
            SetupPermissionToReturn<ViewThing>(false);

            new Action(() => ChangePropertyInPage(page, p => p.Template.PropertyOne)).ShouldThrow<UnauthorizedException>();
        }

        [Test]
        public void PropertyWithoutHandler_RequirePermission_ShouldWorkWhenPermissionIsGranted()
        {
            var page = CreatePage<ExamplePage>();
            SetupPermissionToReturn<ViewThing>(true);

            ChangePropertyInPage(page, p => p.Template.PropertyOne);

            page.PropertyOne.Should().Be(1);
        }

        [Test]
        public void PropertyWithoutHandler_RequirePermissionData_ShouldAskForProperPermission()
        {
            var thing = new Thing();
            var page = CreatePage<ExampleDataPage>();
            page.Data = thing;
            _authEnforcementMock.Setup(enforcement => enforcement.CheckPermission(It.Is<ViewSpecificThing>(permission => permission.Thing == thing)))
                .Returns(true)
                .Verifiable();

            ChangePropertyInPage(page, p => p.Template.PropertyOne);

            _authEnforcementMock.Verify();
        }

        [Test]
        public void PropertyWithoutHandler_RequirePermissionData_ShouldThrowWhenPermissionIsDenied()
        {
            var thing = new Thing();
            var page = CreatePage<ExampleDataPage>();
            page.Data = thing;
            SetupPermissionToReturn<ViewSpecificThing>(false);

            new Action(() => ChangePropertyInPage(page, p => p.Template.PropertyOne)).ShouldThrow<UnauthorizedException>();
        }

        [Test]
        public void PropertyWithoutHandler_RequirePermissionData_ShouldWorkWhenPermissionIsGranted()
        {
            var thing = new Thing();
            var page = CreatePage<ExampleDataPage>();
            page.Data = thing;
            SetupPermissionToReturn<ViewSpecificThing>(true);

            ChangePropertyInPage(page, p => p.Template.PropertyOne);

            page.PropertyOne.Should().Be(1);
        }


        [Test]
        public void SubpageHandlerWithNoAttribute_RequirePermission_ShouldAskForProperPagePermission()
        {
            var page = CreatePage<ExamplePage>().Elements.Add();
            _authEnforcementMock.Setup(enforcement => enforcement.CheckPermission(It.IsAny<ViewThing>()))
                .Returns(true)
                .Verifiable();

            ChangePropertyInPage(page, p => p.Template.ChangeSubThing);

            _authEnforcementMock.Verify();
        }

        [Test]
        public void SubpageHandlerWithNoAttribute_RequirePermission_ShouldThrowWhenPagePermissionIsRejected()
        {
            var page = CreatePage<ExamplePage>().Elements.Add();
            _authEnforcementMock.Setup(enforcement => enforcement.CheckPermission(It.IsAny<ViewThing>()))
                .Returns(false);

            new Action(() => ChangePropertyInPage(page, p => p.Template.ChangeSubThing)).ShouldThrow<UnauthorizedException>();
        }

        [Test]
        public void SubpageHandlerWithNoAttribute_RequirePermission_ShouldWorkWhenPagePermissionIsGranted()
        {
            var page = CreatePage<ExamplePage>().Elements.Add();
            _authEnforcementMock.Setup(enforcement => enforcement.CheckPermission(It.IsAny<ViewThing>()))
                .Returns(true);

            ChangePropertyInPage(page, p => p.Template.ChangeSubThing);

            page.ChangeSubThing.Should().Be(1);
        }

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