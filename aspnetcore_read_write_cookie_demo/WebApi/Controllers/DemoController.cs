using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class DemoController : ControllerBase
{
    [HttpGet("cookie/parse/{cookieName}")]
    public IActionResult ParseTargetCookie(string cookieName)
    {
        var cookie = Request.Headers.Cookie;  // same as Request.Headers["Cookie"]

        var singleCookiesList = Array.ConvertAll(cookie.First().Split(';', StringSplitOptions.RemoveEmptyEntries), c => c.Trim());
        var targetCookie = singleCookiesList.FirstOrDefault(c => c.StartsWith(cookieName))?.Replace($"{cookieName}=", string.Empty);

        var result = string.IsNullOrEmpty(targetCookie) ? new Dictionary<string, StringValues>() : QueryHelpers.ParseQuery(targetCookie);
        
        return Ok(result);
    }
    
    [HttpGet("cookie/parse/v2/{cookieName}")]
    public IActionResult ParseTargetCookieV2(string cookieName)
    {
        IRequestCookieCollection requestCookieCollection = Request.Cookies;
        foreach(KeyValuePair<string, string> cookie in requestCookieCollection)
        {
            if (cookie.Key.Equals(cookieName, StringComparison.InvariantCultureIgnoreCase))
            {
                var result = QueryHelpers.ParseQuery(cookie.Value.Trim());

                return Ok(result);
            }
        }

        return Ok("Not Found");
    }
}