using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using URLShort.UrlShort.Core.Data;

namespace UrlShort.Web.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ApiKeyAuthAttribute : Attribute, IAsyncActionFilter
{
    private const string ApiKeyHeaderName = "X-Api-Key";
    private const string UsernameHeaderName = "X-Username";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey) ||
            !context.HttpContext.Request.Headers.TryGetValue(UsernameHeaderName, out var extractedUsername))
        {
            context.Result = new UnauthorizedObjectResult("API Key and Username headers are required");
            return;
        }

        var dbContext = context.HttpContext.RequestServices.GetRequiredService<AppDb>();
        
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Username == extractedUsername.ToString() && u.ApiKey == extractedApiKey.ToString());

        if (user == null)
        {
            context.Result = new UnauthorizedObjectResult("Invalid API Key or Username");
            return;
        }

        context.HttpContext.Items["ApiUserId"] = user.Id;

        await next();
    }
}
