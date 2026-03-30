using MediatR;
using WebAppExam.Application.User.DTOs;

namespace WebAppExam.Application.User.Queries
{
    public class GetAllUsersQuery : IRequest<List<UserResponseDTO>>
    {
        
    }
}