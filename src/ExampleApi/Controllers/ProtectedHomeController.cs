using Authenticatly.Authorization;
using Authenticatly.Utils;
using Microsoft.AspNetCore.Mvc;

namespace ExampleApi.Controllers;

[ApiController]
[Route("[controller]")]
public class ProtectedHomeController : ControllerBase
{

    [HttpGet]
    [AuthenticatlyAuthorize]
    public IActionResult Get()
    {
        if (HttpContext.Items[AuthenticatlyAuthConstants.AUTHORIZED_ATTRIBUTES_KEY] is not Dictionary<string, string> authorizedAttributes)
        {
            return Unauthorized();
        }

        if (!authorizedAttributes["TestType"].Equals("TestValue"))
        {
            return Forbid();
        }

        return Ok("hello");
    }


}
