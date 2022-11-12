using Microsoft.AspNetCore.Identity;

namespace ExampleApi.Models;

public class DotnetAuthIdentityUser : IdentityUser
{
    public List<SomeResource> SomeResources { get; set; }
}
