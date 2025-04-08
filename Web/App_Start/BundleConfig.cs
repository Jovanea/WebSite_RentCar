using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;

namespace Web.App_Start
{
	public class BundleConfig
	{
        public static void RegisterBundles(BundleCollection bundles)
        {

            //Style Bundle
            bundles.Add(new StyleBundle("~/Content/css").Include(
     "~/Content/Site.css"));


            //Script Bundle
            bundles.Add(new ScriptBundle("~/bundles/sitejs").Include(
            "~/Content/static/site.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
            "~/Scripts/jquery.validate*"));

        }
    }
}