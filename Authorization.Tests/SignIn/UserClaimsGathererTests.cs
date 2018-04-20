using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using FluentAssertions;
using NUnit.Framework;
using Starcounter.Authorization.Model;
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
        private Base64ClaimSerializer _claimSerializer;
        private ClaimDb[] _fixtureClaimDbs;
        private string _claimType1;
        private string _claimValue1;
        private string _claimType2;
        private string _claimValue2;

        [SetUp]
        public void SetUp()
        {
            _claimSerializer = new Base64ClaimSerializer();
            _sut = new UserClaimsGatherer(_claimSerializer);
            _user = new User()
            {
                AssociatedClaims = Enumerable.Empty<IClaimDb>(),
                Groups = Enumerable.Empty<IUserGroup>()
            };

            _claimType1 = "t1";
            _claimValue1 = "v1";
            _claimType2 = "t2";
            _claimValue2 = "v2";
            _fixtureClaimDbs = new[] { CreateClaimDb(_claimType1, _claimValue1), CreateClaimDb(_claimType2, _claimValue2)};
        }

        [Test]
        public void ClaimsDirectlyAssociatedWithUserShouldBeGathered()
        {
            _user.AssociatedClaims = _fixtureClaimDbs;

            Excercise();

            ExpectFixtureClaimsPresent();
        }

        [Test]
        public void ClaimsAssociatedWithGroupShouldBeGathered()
        {
            _user.Groups = new[]
            {
                new UserGroup()
                {
                    AssociatedClaims = _fixtureClaimDbs,
                    SubGroups = Enumerable.Empty<IUserGroup>()

                },
            };

            Excercise();

            ExpectFixtureClaimsPresent();
        }

        [Test]
        public void ClaimsAssociatedWithSubGroupShouldBeGathered()
        {
            _user.Groups = new[]
            {
                new UserGroup()
                {
                    AssociatedClaims = Enumerable.Empty<IClaimDb>(),
                    SubGroups = new []{new UserGroup()
                    {
                        AssociatedClaims = _fixtureClaimDbs,
                        SubGroups = Enumerable.Empty<IUserGroup>()
                    }}

                },
            };

            Excercise();

            ExpectFixtureClaimsPresent();
        }

        [Test]
        public void IdenticalClaimsShouldBeGatheredOnlyOnce()
        {
            _user.AssociatedClaims = _fixtureClaimDbs.Concat(_fixtureClaimDbs);

            Excercise();

            _gatheredClaims.Should().HaveCount(_fixtureClaimDbs.Length);
        }

        private ClaimDb CreateClaimDb(string claimType, string claimValue)
        {
            var claimSerialized = _claimSerializer.Serialize(new Claim(claimType, claimValue));
            return new ClaimDb()
            {
                ClaimSerialized = claimSerialized
            };
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