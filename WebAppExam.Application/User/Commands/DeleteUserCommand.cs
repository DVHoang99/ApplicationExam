using MediatR;
using System;
using WebAppExam.Application.Shared;

namespace WebAppExam.Application.User.Commands
{
    public class DeleteUserCommand(string username) : ICommand<Unit>
    {
        public string Username { get; } = username;

    }
}