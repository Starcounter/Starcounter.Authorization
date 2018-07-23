using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Moq;
using NUnit.Framework;
using Starcounter.Authorization.PageSecurity;

namespace Starcounter.Authorization.Tests.PageSecurity
{
    public class AttributeRequirementsResolverTests
    {
        private Mock<IAuthorizationPolicyProvider> _policyProviderMock;
        private AttributeRequirementsResolver _attributeRequirementsResolver;

        [SetUp]
        public void Setup()
        {
            _policyProviderMock = new Mock<IAuthorizationPolicyProvider>();
            _attributeRequirementsResolver = new AttributeRequirementsResolver(_policyProviderMock.Object);
        }

        [Test]
        public async Task IncludesSingleRoleFromAttribute()
        {
            var role = "role";
            var requirements = await _attributeRequirementsResolver.ResolveAsync(new []{new AuthorizeAttribute(){Roles = role}, });
            requirements
                .Cast<RolesAuthorizationRequirement>()
                .Single()
                .AllowedRoles.Should().BeEquivalentTo(new [] { role });
        }

        [Test]
        public async Task IncludesMultipleRolesFromAttribute()
        {
            var role1 = "role1";
            var role2 = "role2";
            var role = $"{role1},{role2}";
            var requirements = await _attributeRequirementsResolver.ResolveAsync(new []{new AuthorizeAttribute(){Roles = role}, });
            requirements
                .Cast<RolesAuthorizationRequirement>()
                .Single()
                .AllowedRoles.Should().BeEquivalentTo(new [] { role1, role2 });
        }

        [Test]
        public async Task IncludesPolicyFromAttribute()
        {
            var policy = "policy";
            var requirement = new DenyAnonymousAuthorizationRequirement();
            _policyProviderMock.Setup(provider => provider.GetPolicyAsync(policy))
                .ReturnsAsync(new AuthorizationPolicy(new[] {requirement}, new string[0]));

            var requirements = await _attributeRequirementsResolver.ResolveAsync(new[] { new AuthorizeAttribute() { Policy = policy}, });

            requirements.Should().BeEquivalentTo(requirement);
        }

        [Test]
        public void ThrowsMeaningfulExceptionWhenPolicyIsNotFound()
        {
            var policy = "policy";
            _policyProviderMock.Setup(provider => provider.GetPolicyAsync(policy))
                .ReturnsAsync((AuthorizationPolicy) null);

            _attributeRequirementsResolver
                .Awaiting(sut => sut.ResolveAsync(new[] { new AuthorizeAttribute { Policy = policy} }))
                .Should()
                .Throw<InvalidOperationException>()
                .WithMessage($"The AuthorizationPolicy named: '{policy}' was not found.")
                ;
        }

        [Test]
        public async Task IncludesDenyAnonymousRequirementFromEmptyAttribute()
        {
            var requirements = await _attributeRequirementsResolver.ResolveAsync(new[] { new AuthorizeAttribute(), });

            requirements.Should().ContainSingle(authorizationRequirement =>
                authorizationRequirement is DenyAnonymousAuthorizationRequirement);
        }
    }

}