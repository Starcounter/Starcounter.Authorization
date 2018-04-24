using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Starcounter.Authorization.Model.Serialization
{
    /// <summary>
    /// Serializes <see cref="ClaimsPrincipal"/> to base64 string.
    /// </summary>
    /// <remarks>
    /// It only serializes authentication type and list of claims for each identity inside the principal.
    /// It does that to avoid a bug in default serialization of ClaimsPrincipal - see
    /// https://developercommunity.visualstudio.com/content/problem/236195/claimsprincipal-deserialization-fails-on-net-frame.html
    /// It's purpose is short-term serialization for authentication ticket storage - the serialization format is not forward-compatible
    /// </remarks>
    public class Base64ClaimsPrincipalSerializer : IStringSerializer<ClaimsPrincipal>
    {
        public ClaimsPrincipal Deserialize(string serialized)
        {
            var memoryStream = new MemoryStream(Convert.FromBase64String(serialized));
            using (var binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))
            {
                var identitiesCount = binaryReader.ReadInt32();
                var identities = new List<ClaimsIdentity>(identitiesCount);
                for (int i = 0; i < identitiesCount; i++)
                {
                    identities.Add(ReadIdentity(binaryReader));
                }
                return new ClaimsPrincipal(identities);
            }
        }

        private ClaimsIdentity ReadIdentity(BinaryReader binaryReader)
        {
            var hasAuthenticationType = binaryReader.ReadBoolean();
            var authenticationType = hasAuthenticationType ? binaryReader.ReadString() : null;

            var claimsCount = binaryReader.ReadInt32();
            var claims = new List<Claim>(claimsCount);
            for (int j = 0; j < claimsCount; j++)
            {
                claims.Add(new Claim(binaryReader));
            }

            return new ClaimsIdentity(claims, authenticationType);
        }

        public string Serialize(ClaimsPrincipal claimsPrincipal)
        {
            var memoryStream = new MemoryStream();
            using (var binaryWriter = new BinaryWriter(memoryStream, Encoding.UTF8))
            {
                var identities = claimsPrincipal.Identities.ToList();
                binaryWriter.Write(identities.Count);
                foreach (var identity in identities)
                {
                    if (identity.AuthenticationType != null)
                    {
                        binaryWriter.Write(true);
                        binaryWriter.Write(identity.AuthenticationType);
                    }
                    else
                    {
                        binaryWriter.Write(false);
                    }

                    var claims = identity.Claims.ToList();
                    binaryWriter.Write(claims.Count);
                    foreach (var claim in claims)
                    {
                        claim.WriteTo(binaryWriter);
                    }
                }
            }
            var inArray = memoryStream.ToArray();
            return Convert.ToBase64String(inArray);
        }
    }
}