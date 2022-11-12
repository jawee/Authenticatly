using Microsoft.AspNetCore.Http;

namespace Authenticatly.HttpResults;
internal class ForbidObjectResult : IResult
{
    private readonly object? _obj;
    public ForbidObjectResult(object? obj)
    {
        _obj = obj;
    }

    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
        return httpContext.Response.WriteAsJsonAsync(_obj);
    }
}

