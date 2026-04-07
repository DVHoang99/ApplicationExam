using MediatR;
using WebAppExam.Application.User.DTOs;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.User.Queries;

public class GetUserByUsernameQueryHandler : IRequestHandler<GetUserByUsernameQuery, UserResponseDTO>
{
    private readonly IUserRepository _userRepository;

    public GetUserByUsernameQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserResponseDTO> Handle(GetUserByUsernameQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);

        return UserResponseDTO.FromResult(user);
    }
}