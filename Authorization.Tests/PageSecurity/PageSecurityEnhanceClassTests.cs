using System;
using System.Linq.Expressions;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Starcounter.Authorization.Core;
using Starcounter.Templates;

namespace Starcounter.Authorization.Tests.PageSecurity
{
    public class PageSecurityEnhanceClassTests
    {
        private Mock<IAuthorizationEnforcement> _authEnforcementMock;
        private Authorization.PageSecurity.PageSecurity _pageSecurity;
        private Mock<Action<object, object>> _checkDeniedMock;

        [SetUp]
        public void Setup()
        {
            _authEnforcementMock = new Mock<IAuthorizationEnforcement>();
            _checkDeniedMock = new Mock<Action<object, object>>();
            _pageSecurity = new Authorization.PageSecurity.PageSecurity(
                _authEnforcementMock.Object,
                (pageType, permissionExpression, pageExpression) => Expression.Invoke(
                    Expression.Constant(_checkDeniedMock.Object),
                    permissionExpression,
                    pageExpression));
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

            ChangePropertyInPage(page, p => p.Template.ChangeThing);

            VerifyUnchanged(page, page.ChangeThing);
        }

        [Test]
        public void HandlerMarkedWithAttribute_RequirePermission_ShouldWorkWhenPermissionIsGranted()
        {
            var page = CreatePage<ExamplePage>();
            SetupPermissionToReturn<ChangeThing>(true);

            ChangePropertyInPage(page, p => p.Template.ChangeThing);

            VerifyChangedAndCheckDeniedHandlerNotCalled(page.ChangeThing);
            VerifyHandlerWorked(page, nameof(page.ChangeThing));
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

            ChangePropertyInPage(page, p => p.Template.ActionNotMarked);

            VerifyUnchanged(page, page.ActionNotMarked);
        }

        [Test]
        public void HandlerWithNoAttribute_RequirePermissionOnPage_ShouldWorkWhenPermissionIsGranted()
        {
            var page = CreatePage<ExamplePage>();
            SetupPermissionToReturn<ViewThing>(true);

            ChangePropertyInPage(page, p => p.Template.ActionNotMarked);

            VerifyChangedAndCheckDeniedHandlerNotCalled(page.ActionNotMarked);
            VerifyHandlerWorked(page, nameof(page.ActionNotMarked));
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

            ChangePropertyInPage(page, p => p.Template.ViewSpecificThing);

            VerifyUnchanged(page, page.ViewSpecificThing);
        }

        [Test]
        public void HandlerMarkedWithAttribute_RequirePermissionData_ShouldWorkWhenPermissionIsGranted()
        {
            var thing = new Thing();
            var page = CreatePage<ExamplePage>();
            page.Data = thing;
            SetupPermissionToReturn<ViewSpecificThing>(true);

            ChangePropertyInPage(page, p => p.Template.ViewSpecificThing);

            VerifyChangedAndCheckDeniedHandlerNotCalled(page.ViewSpecificThing);
            VerifyHandlerWorked(page, nameof(page.ViewSpecificThing));
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

            ChangePropertyInPage(page, p => p.Template.PropertyOne);

            VerifyUnchanged(page, page.PropertyOne);
        }

        [Test]
        public void PropertyWithoutHandler_RequirePermission_ShouldWorkWhenPermissionIsGranted()
        {
            var page = CreatePage<ExamplePage>();
            SetupPermissionToReturn<ViewThing>(true);

            ChangePropertyInPage(page, p => p.Template.PropertyOne);

            VerifyChangedAndCheckDeniedHandlerNotCalled(page.PropertyOne);
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

            ChangePropertyInPage(page, p => p.Template.PropertyOne);

            VerifyUnchanged(page, page.PropertyOne);
        }

        [Test]
        public void PropertyWithoutHandler_RequirePermissionData_ShouldWorkWhenPermissionIsGranted()
        {
            var thing = new Thing();
            var page = CreatePage<ExampleDataPage>();
            page.Data = thing;
            SetupPermissionToReturn<ViewSpecificThing>(true);

            ChangePropertyInPage(page, p => p.Template.PropertyOne);

            VerifyChangedAndCheckDeniedHandlerNotCalled(page.PropertyOne);
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
            var subpage = CreatePage<ExamplePage>().Elements.Add();
            _authEnforcementMock.Setup(enforcement => enforcement.CheckPermission(It.IsAny<ViewThing>()))
                .Returns(false);

            ChangePropertyInPage(subpage, p => p.Template.ChangeSubThing);

            VerifyUnchanged(subpage, subpage.ChangeSubThing);
        }

        [Test]
        public void SubpageHandlerWithNoAttribute_RequirePermission_ShouldWorkWhenPagePermissionIsGranted()
        {
            var subpage = CreatePage<ExamplePage>().Elements.Add();
            _authEnforcementMock.Setup(enforcement => enforcement.CheckPermission(It.IsAny<ViewThing>()))
                .Returns(true);

            ChangePropertyInPage(subpage, p => p.Template.ChangeSubThing);

            VerifyChangedAndCheckDeniedHandlerNotCalled(subpage.ChangeSubThing);
            VerifyHandlerWorked(subpage, nameof(subpage.ChangeSubThing));
        }

        [Test]
        public void ShouldCallCheckDeniedHandlerWhenPermissionIsDenied()
        {
            var page = CreatePage<ExamplePage>();
            _authEnforcementMock.Setup(enforcement => enforcement.CheckPermission(It.IsAny<ViewThing>()))
                .Returns(false);

            ChangePropertyInPage(page, p => p.Template.ActionNotMarked);

            _checkDeniedMock.Verify(action => action(It.IsAny<ViewThing>(), page), Times.Once());
        }

        private void VerifyChangedAndCheckDeniedHandlerNotCalled(long property)
        {
            property.Should().Be(1);
            _checkDeniedMock.Verify(action => action(It.IsAny<object>(), It.IsAny<object>()), Times.Never());
        }

        private void VerifyHandlerWorked(IExamplePage page, string nameOfProperty)
        {
            page.Changed.Should().Be(nameOfProperty);
        }

        private void VerifyUnchanged(IExamplePage page, long property)
        {
            property.Should().Be(0);
            page.Changed.Should().BeNull();
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