using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ExampleApi.Models;
using ExampleApi.Context;
using ExampleApi.Services;
using Authenticatly;
using Authenticatly.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddIdentity<DotnetAuthIdentityUser, DotnetAuthIdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.SignIn.RequireConfirmedPhoneNumber = true;
    options.SignIn.RequireConfirmedEmail = true;
}).AddEntityFrameworkStores<MyDbContext>().AddDefaultTokenProviders().AddAuthenticatlyTokenProviders<DotnetAuthIdentityUser>("Authenticatly");

builder.Services.AddAuthenticatlyAuthentication<DotnetAuthIdentityUser>(options =>
{
    options.Issuer = "Authenticatly";
    options.TokenValidTimeInMinutes = 10;
    options.Audience = "Authenticatly.ExampleApi";
    options.SymmetricSecurityKey = "mysupersecret_secretkey!123";
    options.AllowedRolesString = "PowerUser;Receptionist";
});

builder.Services.AddTransient<IMfaTokenService, MfaTokenService>();
builder.Services.AddTransient<ISendSmsService, SmsService>();
builder.Services.AddTransient<IClaimsInjectionService, ClaimsInjectionService>();


// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers();


var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
    //db.Database.Migrate();
    //var userManager = scope.ServiceProvider.GetRequiredService<UserManager<DotnetAuthIdentityUser>>();
    //var user = await userManager.FindByEmailAsync("some@email.com");

    //for (var i = 0; i < 5; i++)
    //{
    //    var resource = new SomeResource
    //    {
    //        Name = $"SomeResource {i}",
    //        User = user
    //    };
    //    db.Add(resource);
    //}
    //db.SaveChanges();

    //db.UserTokens.Add(new IdentityUserToken<string> { LoginProvider = "SuperProvider", Name = "SuperName", UserId = user.Id, Value = "arandomstring" });
    //db.SaveChanges();

        //var rolemanager = scope.ServiceProvider.GetRequiredService<RoleManager<DotnetAuthIdentityRole>>();
        //await rolemanager.CreateAsync(new DotnetAuthIdentityRole { Name = "PowerUser" });
        //await rolemanager.CreateAsync(new DotnetAuthIdentityRole { Name = "SuperUser" });
        //await rolemanager.CreateAsync(new DotnetAuthIdentityRole { Name = "Receptionist" });
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/protected", async (HttpContext context) =>
{
    return await Task.FromResult("hello world");
}).RequireAuthenticatlyAuth();

app.AddAuthenticatlyMiddleware();
app.AddAuthenticatlyAuthEndpointsV1<DotnetAuthIdentityUser>();

app.MapControllers();

app.Run();
