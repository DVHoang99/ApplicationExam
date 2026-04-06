using WebAppExam.Application.Auth.DTOs;
using WebAppExam.Application.Shared;

namespace WebAppExam.Application.Auth.Commands;

public class LoginCommand : ICommand<TokenDTO>
{
    public required string Username { get; set; }
    public required string Password { get; set; }

    public static LoginCommand Login(string userName, string password)
    {
        return new LoginCommand
        {
            Username = userName,
            Password = password
        };
    }
}
