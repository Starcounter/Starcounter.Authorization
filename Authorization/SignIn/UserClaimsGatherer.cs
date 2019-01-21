using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Starcounter.Authorization.Model;
using Starcounter.Authorization.Model.Serialization;

namespace Starcounter.Authorization.SignIn
{
    internal class UserClaimsGatherer : IUserClaimsGatherer
    {
        private readonly IClaimDbConverter _claimDbConverter;

        public UserClaimsGatherer(IClaimDbConverter claimDbConverter)
        {
            _claimDbConverter = claimDbConverter;
        }

        public IEnumerable<Claim> Gather(IUser user)
        {
            var dbClaims = new HashSet<IClaimTemplate>();

            foreach (var claimDb in user.AssociatedClaims)
            {
                dbClaims.Add(claimDb);
            }

            foreach (var userGroup in user.MemberOf)
            {
                AddClaimsFromGroup(userGroup, dbClaims);
            }

            var claims = dbClaims.Select(_claimDbConverter.Unpack).ToList();
            claims.Add(new Claim(ClaimTypes.Name, user.Username));

            return claims;
        }

        private void AddClaimsFromGroup(IGroup @group, ICollection<IClaimTemplate> claimsSet)
        {
            foreach (var claimDb in @group.AssociatedClaims)
            {
                claimsSet.Add(claimDb);
            }

            foreach (var subGroup in @group.SubGroups)
            {
                AddClaimsFromGroup(subGroup, claimsSet);
            }
        }
    }
}