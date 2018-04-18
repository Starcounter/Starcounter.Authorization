using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Starcounter.Authorization.Core;
using Starcounter.Authorization.PageSecurity;
using Starcounter.Authorization.Tests.PageSecurity.Fixtures;
using Starcounter.Authorization.Tests.PageSecurity.Utils.TestUtils;
using Starcounter.Authorization.Tests.TestModel;
using static Starcounter.Authorization.Tests.PageSecurity.Utils.TestUtils.AuthorizationEnforecementMockUtils;

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
                new CheckersCreator(_authEnforcementMock.Object, new AttributeRequirementsResolver(new EmptyPolicyProvider())), 
                Options.Create(new SecurityMiddlewareOptions()));
        }

        [Test]
        public async Task CheckClassAsksForProperPermission_RequirePermission()
        {
            await _pageSecurity.CheckClass(typeof(ExamplePage), null);

            _authEnforcementMock.Verify(CheckRequirementsCallWithRoleAndResource(Roles.ThingViewer, (object)null));
        }

        [Test]
        public async Task CheckClassAsksForProperPermission_RequirePermissionData()
        {
            var thing = new Thing();

            await _pageSecurity.CheckClass(typeof(ExampleDataPage), thing);

            _authEnforcementMock.Verify(CheckRequirementsCallWithRoleAndResource(Roles.SpecificThingViewer, thing));
        }

        [Test]
        public async Task CheckClassReturnsFalseWhenPassedNull_RequirePermissionData()
        {
            var checkResult = await _pageSecurity.CheckClass(typeof(ExampleDataPage), new Thing());
            checkResult.Should().BeFalse();
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task CheckClassReturnsResultOfTheCheck_RequirePermissionData(bool expectedOutcome)
        {
            _authEnforcementMock.Setup(CheckRequirementsCallWithRole(Roles.SpecificThingViewer))
                .ReturnsAsync(expectedOutcome);

            var checkResult = await _pageSecurity.CheckClass(typeof(ExampleDataPage), new Thing());
            checkResult
                .Should().Be(expectedOutcome);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task CheckClassReturnsResultOfTheCheck_RequirePermission(bool expectedOutcome)
        {
            _authEnforcementMock.Setup(CheckRequirementsCallWithRoleAndResource(Roles.ThingViewer, (object) null))
                .ReturnsAsync(expectedOutcome);

            var checkResult = await _pageSecurity.CheckClass(typeof(ExamplePage), null);
                checkResult.Should().Be(expectedOutcome);
        }

        [Ignore("This currently doesn't work at all")]
        public void CheckClassUsesCustomCheck()
        {
//            var permission = new ViewThing();
//            _authEnforcementMock.Setup(enforcement => enforcement.CheckPermission(permission))
//                .Returns(true)
//                .Verifiable();
//
//            _pageSecurity.CheckClass(typeof(ExampleCustomPage), permission);
//
//            _authEnforcementMock.Verify();
        }
    }
}