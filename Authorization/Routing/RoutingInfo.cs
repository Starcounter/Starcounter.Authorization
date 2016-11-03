using System;

namespace Starcounter.Authorization.Routing
{
    public class RoutingInfo
    {
        public Type SelectedPageType { get; set; }
        public Request Request { get; set; }
        public string[] Arguments { get; set; }
    }
}