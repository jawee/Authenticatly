# Authenticatly

OAuth 2.0-like authentication support for dotnet apis using AspNetCore.Identity.
Adds `/auth/v1/login` and `/auth/v1/logout` endpoints. 

## Usage in your own endpoints

To get CustomerId and Email from claims in the endpoint.
```csharp
if (HttpContext.Items[AuthenticatlyAuthConstants.AUTHORIZED_ATTRIBUTES_KEY] is not Dictionary<string, string> authorizedAttributes)
{
    return Unauthorized();
}

//Get the values set from claims
var customerId = authorizedAttributes["CustomerId"];
var email = authorizedAttributes["Email"];
```

### Protected endpoints
On MinimalApis, you can use the `.RequireAuthenticatlyAuth` extension method.
```csharp
app.MapGet("/protected", async (HttpContext context) =>
{
    return await Task.FromResult("hello world");
}).RequireAuthenticatlyAuth();
```

On Controllers, you can use `[AuthenticatlyAuthorize]` attribute.
```csharp
[AuthenticatlyAuthorize]
public async Task<IActionResult> Get()
{
    var user = User;
    return Ok("hello");
}
```

## Endpoints
### POST /auth/login
Logs in a user. If 2FA is required, it needs to be setup elsewhere. Only supports TOTP.
#### Request
```json
{
    "email": "string",
    "password": "string",
    "totp": "string",
    "refreshToken": "string"
}
```

#### Response
```json
{
    "accesstoken": "string",
    "tokentype": "bearer",
    "expiresin": "in",
    "scope": [],
    "refreshtoken": "string"
}
```

### POST /auth/logout
Removes the refresh token from the database. 

## Setup
### Configuration
```csharp
var issuer = "Authenticatly";
builder.Services.AddIdentity<DotnetAuthIdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
}).AddEntityFrameworkStores<MyDbContext>().AddDefaultTokenProviders().AddAuthenticatlyTokenProviders<DotnetAuthIdentityUser>(issuer);

builder.Services.AddAuthenticatlyAuthentication<DotnetAuthIdentityUser>(options =>
{
    options.Issuer = issuer;
    options.TokenValidTimeInMinutes = 10;
    options.Audience = "Authenticatly.DotnetAuth";
    options.SymmetricSecurityKey = "mysupersecret_secretkey!123";
    options.AllowedRolesString = "Power User;Receptionist";
});
```

### Required implementations
The interfaces `IMfaTokenService`, `ISendSmsService` and `IClaimsInjectionService` must be implemented and registered in the DI container. 

For example implementations, see `ExampleApi/Services/MfaTokenService.cs`, `ExampleApi/Services/SmsService.cs` and `ExampleApi/Services/ClaimsInjectionService.cs`.