using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using FluentAssertions;
using NUnit.Framework;
using Starcounter.Authorization.Model.Serialization;
using Starcounter.Authorization.SignIn;
using Starcounter.Authorization.Tests.TestModel;

namespace Starcounter.Authorization.Tests.SignIn
{
    public class UserClaimsGathererTests
    {
        private UserClaimsGatherer _sut;
        private User _user;
        private IEnumerable<Claim> _gatheredClaims;
        private ClaimDbConverter _claimDbConverter;
        private ClaimTemplate[] _fixtureClaimTemplates;
        private string _claimType1;
        private string _claimValue1;
        private string _claimType2;
        private string _claimValue2;

        [SetUp]
        public void SetUp()
        {
            _claimDbConverter = new ClaimDbConverter();
            _sut = new UserClaimsGatherer(_claimDbConverter);
            _user = new User()
            {
                AssociatedClaims = Enumerable.Empty<ClaimTemplate>(),
                MemberOf = Enumerable.Empty<IGroup>()
            };

            _claimType1 = "t1";
            _claimValue1 = "v1";
            _claimType2 = "t2";
            _claimValue2 = "v2";
            _fixtureClaimTemplates = new[] { CreateClaimDb(_claimType1, _claimValue1), CreateClaimDb(_claimType2, _claimValue2)};
        }

        [Test]
        public void ClaimsDirectlyAssociatedWithUserShouldBeGathered()
        {
            _user.AssociatedClaims = _fixtureClaimTemplates;

            Excercise();

            ExpectFixtureClaimsPresent();
        }

        [Test]
        public void ClaimsAssociatedWithGroupShouldBeGathered()
        {
            _user.MemberOf = new[]
            {
                new Group()
                {
                    AssociatedClaims = _fixtureClaimTemplates,
                    SubGroups = Enumerable.Empty<IGroup>()

                },
            };

            Excercise();

            ExpectFixtureClaimsPresent();
        }

        [Test]
        public void ClaimsAssociatedWithSubGroupShouldBeGathered()
        {
            _user.MemberOf = new[]
            {
                new Group()
                {
                    AssociatedClaims = Enumerable.Empty<ClaimTemplate>(),
                    SubGroups = new []{new Group()
                    {
                        AssociatedClaims = _fixtureClaimTemplates,
                        SubGroups = Enumerable.Empty<IGroup>()
                    }}

                },
            };

            Excercise();

            ExpectFixtureClaimsPresent();
        }

        [Test]
        public void IdenticalClaimsShouldBeGatheredOnlyOnce()
        {
            _user.AssociatedClaims = _fixtureClaimTemplates.Concat(_fixtureClaimTemplates);

            Excercise();

            _gatheredClaims.Should().HaveCount(_fixtureClaimTemplates.Length);
        }

        private ClaimTemplate CreateClaimDb(string claimType, string claimValue)
        {
            var claimDb = new ClaimTemplate();
            _claimDbConverter.Pack(new Claim(claimType, claimValue), claimDb);
            return claimDb;
        }

        private void Excercise()
        {
            _gatheredClaims = _sut.Gather(_user);
        }

        private void ExpectFixtureClaimsPresent()
        {
            _gatheredClaims.Should()
                .Contain(claim => claim.Type == _claimType1 && claim.Value == _claimValue1)
                .And.Contain(claim => claim.Type == _claimType2 && claim.Value == _claimValue2);
        }
    }
}