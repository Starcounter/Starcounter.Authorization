using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Moq;
using Starcounter.Authorization.Core;

namespace Starcounter.Authorization.Tests.PageSecurity.Utils.TestUtils
{
    public static class AuthorizationEnforecementMockUtils
    {

        public static bool RequiresSingleRole(IEnumerable<IAuthorizationRequirement> requirements,
            string expectedRole) =>
            requirements
                .Cast<RolesAuthorizationRequirement>().Single()
                .AllowedRoles.Single() == expectedRole;


        public static Expression<Func<IAuthorizationEnforcement, Task<bool>>> CheckRequirementsCallWithRoleAndResource<
            TResource>(
            string expectedRole, TResource expectedResource)
        {
            return enforcement => enforcement.CheckRequirementsAsync(
                It.Is<IEnumerable<IAuthorizationRequirement>>(requirements =>
                    RequiresSingleRole(requirements, expectedRole)),
                expectedResource);
        }

        public static Expression<Func<IAuthorizationEnforcement, Task<bool>>> CheckRequirementsCallWithRole(
            string expectedRole)
        {
            return enforcement => enforcement.CheckRequirementsAsync(
                It.Is<IEnumerable<IAuthorizationRequirement>>(requirements =>
                    RequiresSingleRole(requirements, expectedRole)),
                It.IsAny<object>());
        }
    }
}