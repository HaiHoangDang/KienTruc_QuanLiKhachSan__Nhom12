using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace DKS_HotelManager.Helpers
{
    public class CustomerAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (httpContext == null || httpContext.Session == null)
            {
                return false;
            }

            return httpContext.Session["KhachHang"] != null;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var urlHelper = new UrlHelper(filterContext.RequestContext);
            var request = filterContext.HttpContext.Request;
            var rawUrl = request?.RawUrl;

            if ((string.Equals(filterContext.ActionDescriptor.ActionName, "BookRoom", StringComparison.OrdinalIgnoreCase)
                || string.Equals(filterContext.ActionDescriptor.ActionName, "ReviewBooking", StringComparison.OrdinalIgnoreCase))
                && request?.UrlReferrer != null)
            {
                rawUrl = request.UrlReferrer.PathAndQuery;
            }

            var returnUrl = string.IsNullOrWhiteSpace(rawUrl) || !urlHelper.IsLocalUrl(rawUrl)
                ? urlHelper.Action("Index", "Home")
                : rawUrl;

            filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary(
                    new
                    {
                        controller = "Authentication",
                        action = "Login",
                        returnUrl
                    }));
        }
    }
}
