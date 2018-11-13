using System;
using System.Linq.Expressions;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Starcounter.Authorization.PageSecurity;
using Starcounter.Authorization.Tests.PageSecurity.Fixtures;
using Starcounter.Authorization.Tests.PageSecurity.Utils.TestUtils;
using Starcounter.Templates;

namespace Starcounter.Authorization.Tests.PageSecurity
{
    public class EnhanceClassTestsBase
    {
        protected Mock<IAuthorizationEnforcement> AuthEnforcementMock;
        private protected Authorization.PageSecurity.PageSecurity PageSecurity;
        protected Mock<Action<object, object>> CheckDeniedMock;

        [SetUp]
        public void Setup()
        {
            AuthEnforcementMock = new Mock<IAuthorizationEnforcement>();
            CheckDeniedMock = new Mock<Action<object, object>>();
            PageSecurity = CreatePageSecurity(new CheckersCache());
        }

        private protected Authorization.PageSecurity.PageSecurity CreatePageSecurity(CheckersCache checkersCache)
        {
            Func<Type, Expression, Expression, Expression> checkDeniedHandler =
                (pageType, permissionExpression, pageExpression) => Expression.Invoke(
                    Expression.Constant(CheckDeniedMock.Object),
                    permissionExpression,
                    pageExpression);
            return new Authorization.PageSecurity.PageSecurity(
                new CheckersCreator(AuthEnforcementMock.Object,
                    new AttributeRequirementsResolver(new EmptyPolicyProvider()),
                    checkersCache),
                Options.Create(new SecurityMiddlewareOptions().WithCheckDeniedHandler(checkDeniedHandler)));
        }

        protected void VerifyChangedAndCheckDeniedHandlerNotCalled(long property)
        {
            property.Should().Be(1, "property was supposed to be changed");
            CheckDeniedMock.Verify(action => action(It.IsAny<object>(), It.IsAny<object>()), Times.Never());
        }

        protected void VerifyHandlerWorked(IExamplePage page, string nameOfProperty)
        {
            page.Changed.Should().Be(nameOfProperty);
        }

        protected void VerifyUnchanged(IExamplePage page, long property)
        {
            property.Should().Be(0);
            page.Changed.Should().BeNull();
        }

        protected void ChangePropertyInPage<T>(T page, Func<T, Property<long>> propertySelector) where T:Json
        {
            propertySelector(page).ProcessInput(page, 1);
        }

        protected T CreatePage<T>() where T : new()
        {
            PageSecurity.EnhanceClass(typeof(T));
            return new T();
        }

        protected void SetupPolicyToReturn(string policyName, bool valueToReturn)
        {
            AuthEnforcementMock.Setup(AuthorizationEnforecementMockUtils.CheckRequirementsCallWithRole(policyName))
                .ReturnsAsync(valueToReturn)
                .Verifiable();
        }
    }
}