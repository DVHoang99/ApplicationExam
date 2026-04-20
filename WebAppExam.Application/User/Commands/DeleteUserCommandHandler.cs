using FluentResults;
using MediatR;
using WebAppExam.Application.Common.Errors;
using WebAppExam.Domain.Exceptions;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.User.Commands;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result<Ulid>>
{
    private readonly IUserRepository _userRepository;

    public DeleteUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<Ulid>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException($"User {request.Username} not found.");
        }

        user.DeleteUser();

        _userRepository.Update(user);

        return user.Id;
    }
}