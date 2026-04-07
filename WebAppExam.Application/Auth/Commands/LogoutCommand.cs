using System;
using FluentResults;
using WebAppExam.Application.Shared;

namespace WebAppExam.Application.Auth.Commands;

public class LogoutCommand(string username) : ICommand<Result<string>>
{
    public string Username { get; private set; } = username;
    public static LogoutCommand Logout(string username)
    {
        return new LogoutCommand(username);
    }
}
