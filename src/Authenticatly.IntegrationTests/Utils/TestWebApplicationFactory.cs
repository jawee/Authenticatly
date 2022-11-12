using ExampleApi.Context;
using ExampleApi.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Authenticatly.IntegrationTests.Utils;

internal class TestWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        var root = new InMemoryDatabaseRoot();
        builder.ConfigureServices(services =>
        {
            services.AddScoped(sp =>
            {
                return new DbContextOptionsBuilder<MyDbContext>().UseInMemoryDatabase("Tests", root).UseApplicationServiceProvider(sp).Options;
                //return new DbContextOptionsBuilder<MyDbContext>().UseSqlite("DataSource=:memory:").UseApplicationServiceProvider(sp).Options;
            });


            using var scope = services.BuildServiceProvider().CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<MyDbContext>();

            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            var userManager = scopedServices.GetRequiredService<UserManager<DotnetAuthIdentityUser>>();

            var user = new DotnetAuthIdentityUser
            {
                Email = TestConstants.MfaUsername,
                UserName = TestConstants.MfaUsername,
                PhoneNumber = "12345678",
                TwoFactorEnabled = true,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
            };

            var res1 = userManager.CreateAsync(user, TestConstants.Password).Result;

            var user2 = new DotnetAuthIdentityUser
            {
                Email = TestConstants.Username,
                UserName = TestConstants.Username,
                PhoneNumber = "87654321",
                TwoFactorEnabled = false,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };

            var res2 = userManager.CreateAsync(user2, TestConstants.Password).Result;

            var roleManager = scopedServices.GetRequiredService<RoleManager<DotnetAuthIdentityRole>>();
            var roleRes = roleManager.CreateAsync(new DotnetAuthIdentityRole { Name = "PowerUser" }).Result;

            userManager.AddToRoleAsync(user, "PowerUser").Wait();
            userManager.AddToRoleAsync(user2, "PowerUser").Wait();
        });

        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        //using(var scope = builder.Services.CreateScope())
        //{
        //    var scopedServices = scope.ServiceProvider;
        //    var db = scopedServices.GetRequiredService<MyDbContext>();

        //    var userManager = scopedServices.GetRequiredService<UserManager<DotnetAuthIdentityUser>>();

        //    var user = new DotnetAuthIdentityUser
        //    {
        //        Email = "2fa.user@email.com",
        //        UserName = "2fa.user@email.com",
        //        PhoneNumber = "12345678",
        //        TwoFactorEnabled = true,
        //        EmailConfirmed = true,
        //        PhoneNumberConfirmed = true,
        //    };

        //    var res1 = userManager.CreateAsync(user, "Password123!").Result;
        //}

    }
}
