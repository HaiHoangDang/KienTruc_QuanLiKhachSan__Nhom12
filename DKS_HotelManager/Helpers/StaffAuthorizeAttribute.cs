using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using DKS_HotelManager.Models;

namespace DKS_HotelManager.Helpers
{
    public class StaffAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext?.Session == null)
            {
                return false;
            }

            return httpContext.Session["AdminUser"] is NHANVIEN;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var returnUrl = filterContext.HttpContext?.Request?.RawUrl;
            filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary
                {
                    { "area", "Staff" },
                    { "controller", "Auth" },
                    { "action", "Login" },
                    { "returnUrl", returnUrl }
                });
        }
    }
}
