using System;
using System.Windows.Input;
using FluentResults;
using WebAppExam.Application.Shared;

namespace WebAppExam.Application.User.Commands;

public class CreateUserCommand(string username, string password, string role, string name) : ICommand<Result<Ulid>>
{
    public string Username { get; private set; } = username;
    public string Password { get; private set; } = password;
    public string Role { get; private set; } = role;
    public string Name { get; private set; } = name;

    public static CreateUserCommand Init(string username, string password, string role, string name)
    {
        return new CreateUserCommand(username, password, role, name);
    }
}
