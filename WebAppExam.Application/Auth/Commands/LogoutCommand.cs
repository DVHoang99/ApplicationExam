using System;
using WebAppExam.Application.Shared;

namespace WebAppExam.Application.Auth.Commands;

public class LogoutCommand : ICommand<string>
{
    public required string Username { get; set; }
    public static LogoutCommand Logout(string userName)
    {
        return new LogoutCommand
        {
            Username = userName
        };
    }
}
