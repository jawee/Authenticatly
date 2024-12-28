namespace ExampleApi.Models;

public class SomeResource
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string UserId { get; set; }
    public DotnetAuthIdentityUser? User { get; set; }
}
