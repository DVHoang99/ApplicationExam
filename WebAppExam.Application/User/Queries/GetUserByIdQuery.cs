using MediatR;
using System;
using WebAppExam.Application.User.DTOs;

namespace WebAppExam.Application.User.Queries
{
    public class GetUserByIdQuery : IRequest<UserResponseDTO>
    {
        public Ulid Id { get; }

        public GetUserByIdQuery(Ulid id)
        {
            Id = id;
        }
    }
}