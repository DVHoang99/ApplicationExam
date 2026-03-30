using MediatR;
using System;
using WebAppExam.Application.Shared;

namespace WebAppExam.Application.User.Commands
{
    public class UpdateUserCommand(string username) : ICommand<Ulid>
    {
        public string Username { get; } = username;
        public string Name { get; set; }
        public string Role { get; set; }
        public string Password { get; set; }
        
    }
}