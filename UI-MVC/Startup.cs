using Microsoft.Owin;
using Owin;
using System;
using System.Reflection;
using SS.UI.Web.MVC;

[assembly: OwinStartup(typeof(Startup))]

namespace SS.UI.Web.MVC
{
    /// <summary>
    /// The startup.
    /// </summary>
    public partial class Startup
    {
      /// <summary>
      /// The configuration.
      /// </summary>
      /// <param name="app">
      /// The app.
      /// </param>
      /// 
      //changes
      //Version 0.8.0.4 #testable
        public void Configuration(IAppBuilder app)
        {
            this.ConfigureAuth(app);
        }
    }
}
