using Microsoft.AspNetCore.WebUtilities;

namespace WebApi;

public class UpsertCookieMiddleware
{
    private readonly RequestDelegate _next;

    public UpsertCookieMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        context.Response.OnStarting(UpsertCookie, context);
        
        await _next(context);
    }

    private Task UpsertCookie(object state)
    {
        var context = (HttpContext)state;

        var targetCookieName = "cookie11";
        context.Request.Cookies.TryGetValue(targetCookieName, out var cookieFromRequest);
        if (string.IsNullOrEmpty(cookieFromRequest))
        {
            // no change if the target cookie doesn't exist in request
            return Task.CompletedTask;
        }

        var cookieKvMap = QueryHelpers.ParseQuery(cookieFromRequest);
        
        cookieKvMap["newKey"] = "newVal" + DateTime.Now.Millisecond;

        var cookieString = string.Join("&", cookieKvMap.Select(kv => $"{kv.Key}={kv.Value}"));
        
        context.Response.Cookies.Append(targetCookieName, cookieString);

        return Task.CompletedTask;
    }
}