using Starcounter;

namespace Application
{
    [Database]
    public class ClaimRelation
    {
        public ClaimHolder Subject { get; set; }
        public ClaimTemplate Object { get; set; }
    }
}