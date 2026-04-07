using System;
using System.Text.Json.Serialization;

namespace WebAppExam.Application.Auth.DTOs;

public class TokenDTO
{
    public string AccessToken { get; init; }
    public string RefreshToken { get; init; }

    /// <summary>
    /// Parameterless constructor required for JSON deserialization.
    /// Marked as private to prevent direct instantiation via the 'new' keyword from external code,
    /// enforcing the use of the static <see cref="Create"/> factory method.
    /// The [JsonConstructor] attribute grants the ASP.NET Core framework permission to bypass the private access modifier.
    /// </summary>
    /// 
    [JsonConstructor]
    private TokenDTO() { }

    public static TokenDTO FromResult(string accessToken, string refreshToken)
    {
        return new TokenDTO
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }
}
