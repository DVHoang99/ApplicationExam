
using System.Text.Json.Serialization;

namespace WebAppExam.Application.Auth.DTOs;

public class UserLoginDTO
{
    public string Username { get; init; }
    public string Password { get; init; }

    /// <summary>
    /// Parameterless constructor required for JSON deserialization.
    /// Marked as private to prevent direct instantiation via the 'new' keyword from external code,
    /// enforcing the use of the static <see cref="Create"/> factory method.
    /// The [JsonConstructor] attribute grants the ASP.NET Core framework permission to bypass the private access modifier.
    /// </summary>
    /// 
    [JsonConstructor]
    private UserLoginDTO() { }

    public static UserLoginDTO Init(string username, string password)
    {
        return new UserLoginDTO
        {
            Username = username,
            Password = password
        };
    }
}
