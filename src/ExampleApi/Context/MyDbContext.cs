using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ExampleApi.Models;

namespace ExampleApi.Context;
public class MyDbContext : IdentityDbContext
{
    public DbSet<DotnetAuthIdentityUser> DotnetAuthIdentityUsers { get; set; } = null!;
    public DbSet<DotnetAuthIdentityRole> DotnetAuthIdentityRoles { get; set; } = null!;
    public DbSet<SomeResource> SomeResources { get; set; } = null!;
    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
    {
    }
}

