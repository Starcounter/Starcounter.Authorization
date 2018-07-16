using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;

namespace Starcounter.Authorization.Model.Serialization
{
    internal class ClaimDbConverter : IClaimDbConverter
    {
        public Claim Unpack(IClaimTemplate claimTemplate)
        {
            // properties
            var claim = new Claim(claimTemplate.Type, claimTemplate.Value, claimTemplate.ValueType, claimTemplate.Issuer, claimTemplate.OriginalIssuer);
            if (claimTemplate.PropertiesSerialized != null)
            {
                var memoryStream = new MemoryStream(Convert.FromBase64String(claimTemplate.PropertiesSerialized));
                using (var binaryReader = new BinaryReader(memoryStream))
                {
                    var entriesCount = binaryReader.ReadUInt32();
                    for (int i = 0; i < entriesCount; i++)
                    {
                        var key = binaryReader.ReadString();
                        var value = binaryReader.ReadString();
                        claim.Properties.Add(key, value);
                    }
                }
            }
            return claim;
        }

        public void Pack(Claim claim, IClaimTemplate target)
        {
            target.Type = claim.Type;
            target.Value = claim.Value;
            target.ValueType = claim.ValueType;
            target.Issuer = claim.Issuer;
            target.OriginalIssuer = claim.OriginalIssuer;

            if (claim.Properties.Any())
            {
                var memoryStream = new MemoryStream();
                using (var binaryWriter = new BinaryWriter(memoryStream))
                {
                    binaryWriter.Write((uint)claim.Properties.Count);
                    foreach (var entry in claim.Properties)
                    {
                        binaryWriter.Write(entry.Key);
                        binaryWriter.Write(entry.Value);
                    }
                }

                target.PropertiesSerialized = Convert.ToBase64String(memoryStream.ToArray());
            }
        }
    }
}