using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using Web.App_Start;
using eUseControl.BusinessLogic.DBModel;

namespace Web
{
    public class Global : HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {

            // Inițializarea bazei de date
            Database.SetInitializer(new ApplicationDbInitializer());

            // Code that runs on application startup
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            //Register Bundle Table
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Register custom model binders
            ModelBinders.Binders.Add(typeof(decimal), new DecimalModelBinder());
            ModelBinders.Binders.Add(typeof(decimal?), new DecimalModelBinder());

            // Configure Unity
            UnityConfig.RegisterComponents();
        }
    }
}