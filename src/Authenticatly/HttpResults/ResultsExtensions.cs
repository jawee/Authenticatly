using Microsoft.AspNetCore.Http;

namespace Authenticatly.HttpResults;

internal static class ResultsExtensions
{
    public static IResult ForbidResult(this IResultExtensions resultExtensions, object s)
    {
        ArgumentNullException.ThrowIfNull(resultExtensions);

        return new ForbidObjectResult(s);
    }
}
