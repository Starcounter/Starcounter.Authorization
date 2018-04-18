using System;
using System.IO;
using System.Security.Claims;

namespace Starcounter.Authorization.Model.Serialization
{
    public class Base64ClaimSerializer : IStringSerializer<Claim>
    {
        public Claim Deserialize(string serialized)
        {
            var memoryStream = new MemoryStream(Convert.FromBase64String(serialized));
            using (var binaryReader = new BinaryReader(memoryStream))
            {
                return new Claim(binaryReader);
            }
        }

        public string Serialize(Claim claim)
        {
            var memoryStream = new MemoryStream();
            using (var binaryWriter = new BinaryWriter(memoryStream))
            {
                claim.WriteTo(binaryWriter);
                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }
    }
}