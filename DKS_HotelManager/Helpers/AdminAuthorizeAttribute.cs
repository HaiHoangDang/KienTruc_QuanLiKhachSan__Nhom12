using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace DKS_HotelManager.Helpers
{
    public class AdminAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null || httpContext.Session == null)
            {
                return false;
            }

            return httpContext.Session["AdminUser"] != null;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var returnUrl = filterContext.HttpContext?.Request?.RawUrl;
            filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary
                {
                    { "area", "Admin" },
                    { "controller", "Auth" },
                    { "action", "Login" },
                    { "returnUrl", returnUrl }
                });
        }
    }
}
