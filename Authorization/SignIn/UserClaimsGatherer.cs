using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Starcounter.Authorization.Model;
using Starcounter.Authorization.Model.Serialization;

namespace Starcounter.Authorization.SignIn
{
    public class UserClaimsGatherer : IUserClaimsGatherer
    {
        private readonly IClaimDbConverter _claimDbConverter;

        public UserClaimsGatherer(IClaimDbConverter claimDbConverter)
        {
            _claimDbConverter = claimDbConverter;
        }

        public IEnumerable<Claim> Gather(IUserWithGroups user)
        {
            var dbClaims = new HashSet<IClaimDb>();
            foreach (var claimDb in user.AssociatedClaims)
            {
                dbClaims.Add(claimDb);
            }

            foreach (var userGroup in user.Groups)
            {
                AddClaimsFromGroup(userGroup, dbClaims);
            }

            return dbClaims.Select(_claimDbConverter.Unpack);
        }

        private void AddClaimsFromGroup(IUserGroup userGroup, ICollection<IClaimDb> claimsSet)
        {
            foreach (var claimDb in userGroup.AssociatedClaims)
            {
                claimsSet.Add(claimDb);
            }

            foreach (var subGroup in userGroup.SubGroups)
            {
                AddClaimsFromGroup(subGroup, claimsSet);
            }
        }
    }
}