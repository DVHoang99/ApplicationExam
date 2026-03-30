using MediatR;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.User.Commands;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Unit>
{ 
    private readonly IUserRepository _userRepository;

    public DeleteUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Unit> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);

        if (user == null)
        {
            var failure = new FluentValidation.Results.ValidationFailure("User", "User not found");
            throw new FluentValidation.ValidationException(new[] { failure });
        }

        user.DeleteUser();
        
        _userRepository.Update(user);

        return Unit.Value;
    }
}