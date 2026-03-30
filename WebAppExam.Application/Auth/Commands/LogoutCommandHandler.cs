using System;
using MediatR;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Auth.Commands;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, string>
{
    private readonly IUserRepository _userRepository;

    public LogoutCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<string> Handle(LogoutCommand request, CancellationToken ct)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username, ct);

        if (user == null)
        {
            var failure = new FluentValidation.Results.ValidationFailure("Logout", "User not found");
            throw new FluentValidation.ValidationException(new[] { failure });
        }

        user.UpdateRefeshToken(string.Empty, DateTime.MinValue);

        _userRepository.Update(user);
        return request.Username;
    }
}
