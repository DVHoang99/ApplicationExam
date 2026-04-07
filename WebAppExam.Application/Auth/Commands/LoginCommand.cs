using FluentResults;
using WebAppExam.Application.Auth.DTOs;
using WebAppExam.Application.Shared;

namespace WebAppExam.Application.Auth.Commands;

public class LoginCommand(string username, string password) : ICommand<Result<TokenDTO>>
{
    public string Username { get; private set; } = username;
    public string Password { get; private set; } = password;


    public static LoginCommand Login(string userName, string password)
    {
        return new LoginCommand(userName, password);
    }
}
