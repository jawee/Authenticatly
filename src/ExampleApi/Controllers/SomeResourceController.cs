using Authenticatly.Authorization;
using Authenticatly.Utils;
using ExampleApi.Context;
using ExampleApi.Dtos;
using ExampleApi.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExampleApi.Controllers;

[Route("[controller]")]
[ApiController]
public class SomeResourceController : ControllerBase
{
    private readonly MyDbContext _context;

    public SomeResourceController(MyDbContext context)
    {
        _context = context;
    }

    [AuthenticatlyAuthorize]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SomeResourceDto>>> Get()
    {
        if (HttpContext.Items[AuthenticatlyAuthConstants.AUTHORIZED_ATTRIBUTES_KEY] is not Dictionary<string, string> authorizedAttributes)
        {
            return Unauthorized();
        }

        if (!authorizedAttributes.TryGetValue("UserId", out var userId))
        {
            return Forbid();
        }

        var result = await _context.SomeResources.Where(s => s.UserId == userId).Select(s => s.ToDto()).ToListAsync();
        return Ok(result);
    }

    // GET <SomeResourceController>/5
    [AuthenticatlyAuthorize]
    [HttpGet("{id}")]
    public string Get(int id)
    {
        return "value";
    }

    // POST <SomeResourceController>
    [AuthenticatlyAuthorize]
    [HttpPost]
    public void Post([FromBody] string value)
    {
    }

    // PUT <SomeResourceController>/5
    [AuthenticatlyAuthorize]
    [HttpPut("{id}")]
    public void Put(int id, [FromBody] string value)
    {
    }

    // DELETE <SomeResourceController>/5
    [AuthenticatlyAuthorize]
    [HttpDelete("{id}")]
    public void Delete(int id)
    {
    }
}
