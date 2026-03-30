using System;
using WebAppExam.Application.Shared;

namespace WebAppExam.Application.Auth.Commands;

public class LogoutCommand(string username) : ICommand<string>
{
    public string Username { get; set; } = username;
}
