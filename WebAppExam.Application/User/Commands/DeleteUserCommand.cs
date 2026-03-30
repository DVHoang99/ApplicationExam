using MediatR;
using System;
using WebAppExam.Application.Shared;

namespace WebAppExam.Application.User.Commands
{
    public class DeleteUserCommand : ICommand<Unit>
    {
        public Ulid Id { get; }

        public DeleteUserCommand(Ulid id)
        {
            Id = id;
        }
    }
}