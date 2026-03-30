using System;
using MediatR;
using WebAppExam.Application.Common.Helpers;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.User.Commands;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Ulid>
{
    private readonly IUserRepository _userRepository;

    public CreateUserCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Ulid> Handle(CreateUserCommand request, CancellationToken ct)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username, ct);

        if (user != null)
        {
            var failure = new FluentValidation.Results.ValidationFailure("Username", "Username already exists");
            throw new FluentValidation.ValidationException(new[] { failure });
        }
        var newUser = new Domain.Entity.User(request.Username, PasswordHelper.HashPassword(request.Password), request.Name, request.Role);
        await _userRepository.AddAsync(newUser, ct);

        return newUser.Id;
    }
}
