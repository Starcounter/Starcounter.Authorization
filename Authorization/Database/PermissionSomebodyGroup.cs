using Simplified.Ring1;
using Simplified.Ring2;

namespace Starcounter.Authorization.Database
{
    /// <summary>
    /// Denotes that all members of <see cref="Group"/> should be granted permission described by <see cref="Permission"/> token.
    /// To create use instances of this class use <see cref="PermissionToken.GetForPermissionOrCreate{T}"/>
    /// </summary>
    [Database]
    public class PermissionSomebodyGroup : Relation
    {
        public PermissionToken Permission;

        public SomebodyGroup Group;
    }
}