using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ExampleApi.Models;

namespace ExampleApi.Context;
public class MyDbContext : IdentityDbContext
{
    public DbSet<DotnetAuthIdentityUser> DotnetAuthIdentityUsers { get; set; }
    public DbSet<DotnetAuthIdentityRole> DotnetAuthIdentityRoles { get; set; }
    public DbSet<SomeResource> SomeResources { get; set; }
    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
    {
    }
}

