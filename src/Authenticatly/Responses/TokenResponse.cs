using System;
namespace Authenticatly.Responses;

public record TokenResponse(string AccessToken, string TokenType, int ExpiresIn, string[] scope, string RefreshToken);

