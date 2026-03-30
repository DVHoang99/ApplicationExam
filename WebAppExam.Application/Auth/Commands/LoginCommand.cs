using WebAppExam.Application.Auth.DTOs;
using WebAppExam.Application.Shared;

namespace WebAppExam.Application.Auth.Commands;

public class LoginCommand(string username, string password) : ICommand<TokenDTO>
{
    public string Username { get; set; } = username;
    public string Password { get; set; } = password;
}
