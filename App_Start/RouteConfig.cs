using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

/**
 * Capstone Project 2022
 * Part of the Microsoft ASP.NET Framework MVC 5
 * This class deals with rounting the site resources together (e.g. exposing APIs)
 */
namespace CapstoneApp
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
            name: "ProfileManager",
            url: "Manage/{action}/{itemName}",
            defaults: new { controller = "Manage", action = "Index", itemName = UrlParameter.Optional}
            );

            routes.MapRoute(
            name: "Default",
            url: "{controller}/{action}",
            defaults: new { controller = "Home", action = "Index"}
            );

          
        }
    }
}
