using MediatR;
using System;
using WebAppExam.Application.Shared;

namespace WebAppExam.Application.User.Commands
{
    public class UpdateUserCommand : ICommand<Ulid>
    {
        public Ulid Id { get; }
        public string Name { get; set; }
        public string Role { get; set; }

        public UpdateUserCommand(Ulid id)
        {
            Id = id;
        }
    }
}