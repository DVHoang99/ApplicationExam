using WebAppExam.Application.Shared;
using WebAppExam.Application.User.DTOs;

namespace WebAppExam.Application.User.Queries;

public class GetUserByUsernameQuery(string username) : ICommand<UserResponseDTO>
{
    public string Username { get; } = username;
}
