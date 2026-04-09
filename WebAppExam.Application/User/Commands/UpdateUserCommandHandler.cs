using FluentResults;
using MediatR;
using WebAppExam.Application.Common.Errors;
using WebAppExam.Application.Common.Helpers;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.User.Commands;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result<Ulid>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Ulid>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (user == null)
        {
            return Result.Fail(new NotFoundError("User", request.Username));
        }

        user.UpdateUser(PasswordHelper.HashPassword(request.Password), request.Name, request.Role);
        _userRepository.Update(user);

        return Result.Ok(user.Id);
    }
}
