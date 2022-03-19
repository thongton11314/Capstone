using System.Web;
using System.Web.Mvc;

/**
 * Capstone Project 2022
 * Part of the Microsoft ASP.NET Framework MVC 5
 * This class deals with handling global errors
 */
namespace CapstoneApp
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
