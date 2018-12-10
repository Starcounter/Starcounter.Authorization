using Starcounter;
using Starcounter.Authorization;

namespace Application
{
    [Database]
    public class ClaimTemplate : IClaimTemplate
    {
        public string Type { get; set; }
        public string Value { get; set; }
        public string ValueType { get; set; }
        public string Issuer { get; set; }
        public string OriginalIssuer { get; set; }
        public string PropertiesSerialized { get; set; }
    }
}