using MediatR;
using WebAppExam.Application.Common.Helpers;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.User.Commands;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Ulid>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUserCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Ulid> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (user == null)
        {
            var failure = new FluentValidation.Results.ValidationFailure("User", "User not found");
            throw new FluentValidation.ValidationException(new[] { failure });
        }

        user.UpdateUser(PasswordHelper.HashPassword(request.Password), request.Name, request.Role);
        _userRepository.Update(user);

        return user.Id;
    }
}
