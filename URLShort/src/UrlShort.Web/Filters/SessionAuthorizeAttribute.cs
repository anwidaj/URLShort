using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace UrlShort.Web.Filters;

public class SessionAuthorizeAttribute : ActionFilterAttribute
{
    public string? Roles { get; set; }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var userId = context.HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            context.Result = new RedirectToActionResult("Login", "Auth", null);
            return;
        }

        if (!string.IsNullOrEmpty(Roles))
        {
            var userRole = context.HttpContext.Session.GetString("Role");
            if (userRole == null)
            {
                context.Result = new RedirectToActionResult("Index", "Home", new { error = "AccessDenied" });
                return;
            }

            var allowedRoles = Roles.Split(',').Select(r => r.Trim()).ToList();
            if (!allowedRoles.Contains(userRole))
            {
                context.Result = new RedirectToActionResult("Index", "Home", new { error = "AccessDenied" });
                return;
            }
        }

        base.OnActionExecuting(context);
    }
}
