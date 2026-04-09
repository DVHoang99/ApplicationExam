using FluentResults;
using MediatR;
using System;
using WebAppExam.Application.Shared;

namespace WebAppExam.Application.User.Commands
{
    public class DeleteUserCommand(string username) : ICommand<Result<Ulid>>
    {
        public string Username { get; } = username;

    }
}