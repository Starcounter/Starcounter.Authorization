using System;
using Microsoft.AspNetCore.Authorization;
using Starcounter;
//using Starcounter.Authorization.Routing;

namespace StarcounterApplication1
{
//    [Url("/StarcounterApplication1/main")]
    [Authorize("DoThings")]
    partial class MainPage : Json
    {
        [Authorize("DoThings")]
        public void Handle(Input.DoTrigger input)
        {
            Text = Guid.NewGuid().ToString();
        }
    }
}
