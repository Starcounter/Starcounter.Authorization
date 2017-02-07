using System;

namespace Starcounter.Authorization.Routing
{
    public class RoutingInfo
    {
        /// <summary>
        /// The type that was associated with URL that has been requested
        /// </summary>
        public Type SelectedPageType { get; set; }

        /// <summary>
        /// Original Starcounter request
        /// </summary>
        public Request Request { get; set; }

        /// <summary>
        /// URL arguments
        /// </summary>
        public string[] Arguments { get; set; }

        /// <summary>
        /// The context of the request
        /// </summary>
        /// <seealso cref="IPageContext{T}"/>
        public object Context { get; set; }
    }
}