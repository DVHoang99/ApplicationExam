using FluentResults;
using MediatR;
using System;
using WebAppExam.Application.Shared;

namespace WebAppExam.Application.User.Commands
{
    public class UpdateUserCommand(string username) : ICommand<Result<Ulid>>
    {
        public string Username { get; } = username;
        public string Name { get; private set; }
        public string Role { get; private set; }
        public string Password { get; private set; }

    }
}