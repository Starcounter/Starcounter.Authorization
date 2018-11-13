using Moq;
using NUnit.Framework;
using Starcounter.Authorization.Tests.PageSecurity.Fixtures;
using Starcounter.Authorization.Tests.PageSecurity.Fixtures.Sharing;
using Starcounter.Authorization.Tests.TestModel;
using static Starcounter.Authorization.Tests.PageSecurity.Utils.TestUtils.AuthorizationEnforecementMockUtils;


namespace Starcounter.Authorization.Tests.PageSecurity
{
    public class PageSecurityEnhanceClassTests : EnhanceClassTestsBase
    {
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

            AuthEnforcementMock.Verify();
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

            AuthEnforcementMock.Verify();
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
            AuthEnforcementMock.Setup(CheckRequirementsCallWithRoleAndResource(Roles.SpecificThingViewer, thing))
                .ReturnsAsync(true)
                .Verifiable();

            ChangePropertyInPage(page, p => p.Template.ViewSpecificThing);

            AuthEnforcementMock.Verify();
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
            AuthEnforcementMock.Setup(CheckRequirementsCallWithRole(Roles.ThingViewer))
                .ReturnsAsync(true)
                .Verifiable();

            ChangePropertyInPage(page, p => p.Template.PropertyOne);

            AuthEnforcementMock.Verify();
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
            AuthEnforcementMock.Setup(CheckRequirementsCallWithRoleAndResource(Roles.SpecificThingViewer, thing))
                .ReturnsAsync(true)
                .Verifiable();

            ChangePropertyInPage(page, p => p.Template.PropertyOne);

            AuthEnforcementMock.Verify();
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
            AuthEnforcementMock.Setup(CheckRequirementsCallWithRole(Roles.ThingViewer))
                .ReturnsAsync(true)
                .Verifiable();

            ChangePropertyInPage(page, p => p.Template.ChangeSubThing);

            AuthEnforcementMock.Verify();
        }

        [Test]
        public void SubpageHandlerWithNoAttribute_RequirePermission_ShouldNotWorkWhenPagePermissionIsRejected()
        {
            var subpage = CreatePage<ExamplePage>().Elements.Add();
            AuthEnforcementMock.Setup(CheckRequirementsCallWithRole(Roles.ThingViewer))
                .ReturnsAsync(false);

            ChangePropertyInPage(subpage, p => p.Template.ChangeSubThing);

            VerifyUnchanged(subpage, subpage.ChangeSubThing);
        }

        [Test]
        public void SubpageHandlerWithNoAttribute_RequirePermission_ShouldWorkWhenPagePermissionIsGranted()
        {
            var subpage = CreatePage<ExamplePage>().Elements.Add();
            AuthEnforcementMock.Setup(CheckRequirementsCallWithRole(Roles.ThingViewer))
                .ReturnsAsync(true);

            ChangePropertyInPage(subpage, p => p.Template.ChangeSubThing);

            VerifyChangedAndCheckDeniedHandlerNotCalled(subpage.ChangeSubThing);
            VerifyHandlerWorked(subpage, nameof(subpage.ChangeSubThing));
        }
		
        [Test]
        public void ShouldCallCheckDeniedHandlerWhenPermissionIsDenied()
        {
            var page = CreatePage<ExamplePage>();
            AuthEnforcementMock.Setup(CheckRequirementsCallWithRole(Roles.ThingViewer))
                .ReturnsAsync(false);

            ChangePropertyInPage(page, p => p.Template.ActionNotMarked);

            CheckDeniedMock.Verify(action => action(It.IsAny<object>(), page), Times.Once());
        }

        [Test]
        public void SubpageNestedHandlerMarkedWithAttribute_RequirePermission_ShouldAskForProperPermission()
        {
            var subpage = CreatePage<ExamplePage>().Elements.Add();
            SetupPolicyToReturn(Roles.ThingEditor, true);

            ChangePropertyInPage(subpage, p => p.Template.ChangeSecuredSubThing);

            AuthEnforcementMock.Verify();
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
            AuthEnforcementMock.Setup(CheckRequirementsCallWithRoleAndResource(Roles.SpecificThingViewer, thing))
                .ReturnsAsync(true)
                .Verifiable();

            ChangePropertyInPage(page.PropertyTwo, p => p.Template.SomeProperty);

            AuthEnforcementMock.Verify();
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
            AuthEnforcementMock.Setup(CheckRequirementsCallWithRoleAndResource(Roles.SpecificThingEditor, thing))
                .ReturnsAsync(true)
                .Verifiable();

            ChangePropertyInPage(nestedPage, p => p.Template.SomeSecuredNestedProperty);

            AuthEnforcementMock.Verify();
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
            PageSecurity.EnhanceClass(typeof(CommonPart));
            // no exception thrown
        }
    }
}