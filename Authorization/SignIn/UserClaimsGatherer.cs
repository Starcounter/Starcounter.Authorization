using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Starcounter.Authorization.Model;
using Starcounter.Authorization.Model.Serialization;

namespace Starcounter.Authorization.SignIn
{
    public class UserClaimsGatherer : IUserClaimsGatherer
    {
        private readonly IStringSerializer<Claim> _claimSerializer;

        public UserClaimsGatherer(IStringSerializer<Claim> claimSerializer)
        {
            _claimSerializer = claimSerializer;
        }

        public IEnumerable<Claim> Gather(IUserWithGroups user)
        {
            var serializedClaims = new HashSet<string>();
            foreach (var claimDb in user.AssociatedClaims)
            {
                serializedClaims.Add(claimDb.ClaimSerialized);
            }

            foreach (var userGroup in user.Groups)
            {
                AddClaimsFromGroup(userGroup, serializedClaims);
            }

            return serializedClaims.Select(_claimSerializer.Deserialize);
        }

        private void AddClaimsFromGroup(IUserGroup userGroup, ICollection<string> claimsSet)
        {
            foreach (var claimDb in userGroup.AssociatedClaims)
            {
                claimsSet.Add(claimDb.ClaimSerialized);
            }

            foreach (var subGroup in userGroup.SubGroups)
            {
                AddClaimsFromGroup(subGroup, claimsSet);
            }
        }
    }
}