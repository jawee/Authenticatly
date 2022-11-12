namespace ExampleApi.Models;

public class SomeResource
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string UserId { get; set; }
    public DotnetAuthIdentityUser User { get; set; }
}
