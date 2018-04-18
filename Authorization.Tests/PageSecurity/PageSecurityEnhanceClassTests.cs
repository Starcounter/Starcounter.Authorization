using System;
using System.IO;
using System.Linq.Expressions;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Starcounter.Authorization.Core;
using Starcounter.Authorization.PageSecurity;
using Starcounter.Authorization.Tests.PageSecurity.Fixtures;
using Starcounter.Authorization.Tests.PageSecurity.Utils.TestUtils;
using Starcounter.Authorization.Tests.TestModel;
using Starcounter.Templates;
using static Starcounter.Authorization.Tests.PageSecurity.Utils.TestUtils.AuthorizationEnforecementMockUtils;


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
            Func<Type, Expression, Expression, Expression> checkDeniedHandler = (pageType, permissionExpression, pageExpression) => Expression.Invoke(
                Expression.Constant(_checkDeniedMock.Object),
                permissionExpression,
                pageExpression);
            _pageSecurity = new Authorization.PageSecurity.PageSecurity(
                new CheckersCreator(_authEnforcementMock.Object, new AttributeRequirementsResolver(new EmptyPolicyProvider())), 
                Options.Create(new SecurityMiddlewareOptions().WithCheckDeniedHandler(checkDeniedHandler)));
        }

        [Test]
        public void HandlerMarkedWithAllowAnonymous_ShouldSucceed()
        {
            var page = CreatePage<ExamplePage>();
            SetupPolicyToReturn(Roles.ThingViewer, false);
            
            ChangePropertyInPage(page, p => p.Template.PubliclyAccessibleThing);

            VerifyChangedAndCheckDeniedHandlerNotCalled(page.PubliclyAccessibleThing);
        }

        [Test]
        public void HandlerMarkedWithAttribute_RequirePermission_ShouldAskForProperPermission()
        {
            var page = CreatePage<ExamplePage>();
            SetupPolicyToReturn(Roles.ThingEditor, true);
            
            ChangePropertyInPage(page, p => p.Template.ChangeThing);

            _authEnforcementMock.Verify();
        }

        [Test]
        public void HandlerMarkedWithAttribute_RequirePermission_ShouldThrowWhenPermissionIsDenied()
        {
            var page = CreatePage<ExamplePage>();
            SetupPolicyToReturn(Roles.ThingEditor, false);

            ChangePropertyInPage(page, p => p.Template.ChangeThing);

            VerifyUnchanged(page, page.ChangeThing);
        }

        [Test]
        public void HandlerMarkedWithAttribute_RequirePermission_ShouldWorkWhenPermissionIsGranted()
        {
            var page = CreatePage<ExamplePage>();
            SetupPolicyToReturn(Roles.ThingEditor, true);

            ChangePropertyInPage(page, p => p.Template.ChangeThing);

            VerifyChangedAndCheckDeniedHandlerNotCalled(page.ChangeThing);
            VerifyHandlerWorked(page, nameof(page.ChangeThing));
        }

        [Test]
        public void HandlerWithNoAttribute_RequirePermissionOnPage_ShouldAskForProperPagePermission()
        {
            var page = CreatePage<ExamplePage>();
            SetupPolicyToReturn(Roles.ThingViewer, true);

            ChangePropertyInPage(page, p => p.Template.ActionNotMarked);

            _authEnforcementMock.Verify();
        }

        [Test]
        public void HandlerWithNoAttribute_RequirePermissionOnPage_ShouldNotWorkWhenPagePermissionIsDenied()
        {
            var page = CreatePage<ExamplePage>();
            SetupPolicyToReturn(Roles.ThingViewer, false);

            ChangePropertyInPage(page, p => p.Template.ActionNotMarked);

            VerifyUnchanged(page, page.ActionNotMarked);
        }

        [Test]
        public void HandlerWithNoAttribute_RequirePermissionOnPage_ShouldWorkWhenPermissionIsGranted()
        {
            var page = CreatePage<ExamplePage>();
            SetupPolicyToReturn(Roles.ThingViewer, true);

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
            _authEnforcementMock.Setup(CheckRequirementsCallWithRoleAndResource(Roles.SpecificThingViewer, thing))
                .ReturnsAsync(true)
                .Verifiable();

            ChangePropertyInPage(page, p => p.Template.ViewSpecificThing);

            _authEnforcementMock.Verify();
        }

        [Test]
        public void HandlerMarkedWithAttribute_RequirePermissionData_ShouldNotWorkWhenPermissionIsDenied()
        {
            var thing = new Thing();
            var page = CreatePage<ExamplePage>();
            page.Data = thing;
            SetupPolicyToReturn(Roles.SpecificThingViewer, false);

            ChangePropertyInPage(page, p => p.Template.ViewSpecificThing);

            VerifyUnchanged(page, page.ViewSpecificThing);
        }

        [Test]
        public void HandlerMarkedWithAttribute_RequirePermissionData_ShouldWorkWhenPermissionIsGranted()
        {
            var thing = new Thing();
            var page = CreatePage<ExamplePage>();
            page.Data = thing;
            SetupPolicyToReturn(Roles.SpecificThingViewer, true);

            ChangePropertyInPage(page, p => p.Template.ViewSpecificThing);

            VerifyChangedAndCheckDeniedHandlerNotCalled(page.ViewSpecificThing);
            VerifyHandlerWorked(page, nameof(page.ViewSpecificThing));
        }

        [Test]
        public void PropertyWithoutHandler_RequirePermission_ShouldAskForProperPermission()
        {
            var page = CreatePage<ExamplePage>();
            _authEnforcementMock.Setup(CheckRequirementsCallWithRole(Roles.ThingViewer))
                .ReturnsAsync(true)
                .Verifiable();

            ChangePropertyInPage(page, p => p.Template.PropertyOne);

            _authEnforcementMock.Verify();
        }

        [Test]
        public void PropertyWithoutHandler_RequirePermission_ShouldNotWorkWhenPermissionIsDenied()
        {
            var page = CreatePage<ExamplePage>();
            SetupPolicyToReturn(Roles.ThingViewer, false);

            ChangePropertyInPage(page, p => p.Template.PropertyOne);

            VerifyUnchanged(page, page.PropertyOne);
        }

        [Test]
        public void PropertyWithoutHandler_RequirePermission_ShouldWorkWhenPermissionIsGranted()
        {
            var page = CreatePage<ExamplePage>();
            SetupPolicyToReturn(Roles.ThingViewer, true);

            ChangePropertyInPage(page, p => p.Template.PropertyOne);

            VerifyChangedAndCheckDeniedHandlerNotCalled(page.PropertyOne);
        }

        [Test]
        public void PropertyWithoutHandler_RequirePermissionData_ShouldAskForProperPermission()
        {
            var thing = new Thing();
            var page = CreatePage<ExampleDataPage>();
            page.Data = thing;
            _authEnforcementMock.Setup(CheckRequirementsCallWithRoleAndResource(Roles.SpecificThingViewer, thing))
                .ReturnsAsync(true)
                .Verifiable();

            ChangePropertyInPage(page, p => p.Template.PropertyOne);

            _authEnforcementMock.Verify();
        }

        [Test]
        public void PropertyWithoutHandler_RequirePermissionData_ShouldNotWorkWhenPermissionIsDenied()
        {
            var thing = new Thing();
            var page = CreatePage<ExampleDataPage>();
            page.Data = thing;
            SetupPolicyToReturn(Roles.SpecificThingViewer, false);

            ChangePropertyInPage(page, p => p.Template.PropertyOne);

            VerifyUnchanged(page, page.PropertyOne);
        }

        [Test]
        public void PropertyWithoutHandler_RequirePermissionData_ShouldWorkWhenPermissionIsGranted()
        {
            var thing = new Thing();
            var page = CreatePage<ExampleDataPage>();
            page.Data = thing;
            SetupPolicyToReturn(Roles.SpecificThingViewer, true);

            ChangePropertyInPage(page, p => p.Template.PropertyOne);

            VerifyChangedAndCheckDeniedHandlerNotCalled(page.PropertyOne);
        }


        [Test]
        public void SubpageHandlerWithNoAttribute_RequirePermission_ShouldAskForProperPagePermission()
        {
            var page = CreatePage<ExamplePage>().Elements.Add();
            _authEnforcementMock.Setup(CheckRequirementsCallWithRole(Roles.ThingViewer))
                .ReturnsAsync(true)
                .Verifiable();

            ChangePropertyInPage(page, p => p.Template.ChangeSubThing);

            _authEnforcementMock.Verify();
        }

        [Test]
        public void SubpageHandlerWithNoAttribute_RequirePermission_ShouldNotWorkWhenPagePermissionIsRejected()
        {
            var subpage = CreatePage<ExamplePage>().Elements.Add();
            _authEnforcementMock.Setup(CheckRequirementsCallWithRole(Roles.ThingViewer))
                .ReturnsAsync(false);

            ChangePropertyInPage(subpage, p => p.Template.ChangeSubThing);

            VerifyUnchanged(subpage, subpage.ChangeSubThing);
        }

        [Test]
        public void SubpageHandlerWithNoAttribute_RequirePermission_ShouldWorkWhenPagePermissionIsGranted()
        {
            var subpage = CreatePage<ExamplePage>().Elements.Add();
            _authEnforcementMock.Setup(CheckRequirementsCallWithRole(Roles.ThingViewer))
                .ReturnsAsync(true);

            ChangePropertyInPage(subpage, p => p.Template.ChangeSubThing);

            VerifyChangedAndCheckDeniedHandlerNotCalled(subpage.ChangeSubThing);
            VerifyHandlerWorked(subpage, nameof(subpage.ChangeSubThing));
        }
		
        [Test]
        public void ShouldCallCheckDeniedHandlerWhenPermissionIsDenied()
        {
            var page = CreatePage<ExamplePage>();
            _authEnforcementMock.Setup(CheckRequirementsCallWithRole(Roles.ThingViewer))
                .ReturnsAsync(false);

            ChangePropertyInPage(page, p => p.Template.ActionNotMarked);

            _checkDeniedMock.Verify(action => action(It.IsAny<object>(), page), Times.Once());
        }

        [Test]
        public void SubpageNestedHandlerMarkedWithAttribute_RequirePermission_ShouldAskForProperPermission()
        {
            var subpage = CreatePage<ExamplePage>().Elements.Add();
            SetupPolicyToReturn(Roles.ThingEditor, true);

            ChangePropertyInPage(subpage, p => p.Template.ChangeSecuredSubThing);

            _authEnforcementMock.Verify();
        }

        [Test]
        public void SubpageNestedHandlerMarkedWithAttribute_RequirePermission_ShouldNotWorkWhenPermissionIsDenied()
        {
            var subpage = CreatePage<ExamplePage>().Elements.Add();

            SetupPolicyToReturn(Roles.SpecificThingEditor, false);

            ChangePropertyInPage(subpage, p => p.Template.ChangeSecuredSubThing);

            VerifyUnchanged(subpage, subpage.ChangeSecuredSubThing);
        }

        [Test]
        public void SubpageHandlerWithNoAttribute_RequirePermissionData_ShouldAskForProperPermission()
        {
            var thing = new Thing();
            var page = CreatePage<ExampleDataPage>();
            page.Data = thing;
            _authEnforcementMock.Setup(CheckRequirementsCallWithRoleAndResource(Roles.SpecificThingViewer, thing))
                .ReturnsAsync(true)
                .Verifiable();

            ChangePropertyInPage(page.PropertyTwo, p => p.Template.SomeProperty);

            _authEnforcementMock.Verify();
        }

        // TODO this no longer holds, because we can't determine what type of .Data we are looking for from the policy name
        // that's why the engine can't know it should look into parent's .Data
        [Test]
        [Ignore("This test no longer holds")]
        public void SubpageNestedHandlerMarkedWithAttribute_RequirePermissionData_ShouldAskForProperPermission()
        {
            var thing = new Thing();
            var page = CreatePage<ExampleDataPage>();
            var nestedPage = page.PropertyTwo.NestedElements.Add();
            page.Data = thing;
            nestedPage.Data = new OtherThingItem();
            _authEnforcementMock.Setup(CheckRequirementsCallWithRoleAndResource(Roles.SpecificThingEditor, thing))
                .ReturnsAsync(true)
                .Verifiable();

            ChangePropertyInPage(nestedPage, p => p.Template.SomeSecuredNestedProperty);

            _authEnforcementMock.Verify();
        }

        [Test]
        public void SubpageNestedHandlerMarkedWithAttribute_RequirePermissionData_ShouldNotWorkWhenPermissionIsDenied()
        {
            var thing = new Thing();
            var page = CreatePage<ExampleDataPage>();
            var nestedPage = page.PropertyTwo.NestedElements.Add();
            page.Data = thing;
            nestedPage.Data = new OtherThingItem();

            SetupPolicyToReturn(Roles.SpecificThingEditor, false);

            ChangePropertyInPage(nestedPage, p => p.Template.SomeSecuredNestedProperty);

            VerifyUnchanged(nestedPage, nestedPage.SomeSecuredNestedProperty);
        }

        [Test]
        public void HandlerWithoutAttribute_Unsecured_ShouldAlwaysWork()
        {
            var page = CreatePage<ExampleUnsecuredPage>();

            ChangePropertyInPage(page, p => p.Template.Action1);

            VerifyChangedAndCheckDeniedHandlerNotCalled(page.Action1);
            VerifyHandlerWorked(page, nameof(ExampleUnsecuredPage.Action1));
        }

        [Test]
        public void EmptyArrayDoesntCauseProblems()
        {
            _pageSecurity.EnhanceClass(typeof(EmptyArrayPage));
            // no exception thrown
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

        private void SetupPolicyToReturn(string policyName, bool valueToReturn)
        {
            _authEnforcementMock.Setup(CheckRequirementsCallWithRole(policyName))
                .ReturnsAsync(valueToReturn)
                .Verifiable();
        }

        
    }
}