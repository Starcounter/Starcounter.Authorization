using System;
using Simplified.Ring2;

namespace Starcounter.Authorization.Partial
{
    public class PermissionSelectionPartialEventArgs : EventArgs
    {
        public PermissionSelectionPartialEventArgs(SomebodyGroup group) { SomebodyGroup = group; }
        public SomebodyGroup SomebodyGroup { get; private set; }
    }
}