using System;
using System.IO;
using System.Security.Claims;
using System.Text;

namespace Starcounter.Authorization.Model.Serialization
{
    public class Base64ClaimsPrincipalSerializer : IStringSerializer<ClaimsPrincipal>
    {
        public ClaimsPrincipal Deserialize(string serialized)
        {
            var memoryStream = new MemoryStream(Convert.FromBase64String(serialized));
            using (var binaryReader = new BinaryReader(memoryStream, Encoding.UTF8))
            {
                return new ClaimsPrincipal(binaryReader);
            }
        }

        public string Serialize(ClaimsPrincipal claimsPrincipal)
        {
            var memoryStream = new MemoryStream();
            using (var binaryWriter = new BinaryWriter(memoryStream, Encoding.UTF8))
            {
                claimsPrincipal.WriteTo(binaryWriter);
            }
            var inArray = memoryStream.ToArray();
            return Convert.ToBase64String(inArray);
        }
    }
}