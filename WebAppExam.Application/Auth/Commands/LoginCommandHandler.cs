using MediatR;
using WebAppExam.Application.Auth.DTOs;
using WebAppExam.Application.Auth.Services;
using WebAppExam.Application.Common.Helpers;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Auth.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, TokenDTO>
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthService _authService;

    public LoginCommandHandler(IUserRepository userRepository, IAuthService authService)
    {
        _userRepository = userRepository;
        _authService = authService;
    }

    public async Task<TokenDTO> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await _userRepository.GetByUsernameAsync(request.Username, ct);

        if (user == null || !PasswordHelper.VerifyPassword(request.Password, user.PasswordHash))
        {
            var failure = new FluentValidation.Results.ValidationFailure("Login", "Invalid username or password");
            throw new FluentValidation.ValidationException(new[] { failure });
        }

        var token = _authService.GenerateJwtToken(user.Username, user.Role);
        var refreshToken = _authService.GenerateRefreshToken();

        user.UpdateRefeshToken(refreshToken, DateTime.UtcNow.AddDays(7));

        _userRepository.Update(user);

        return new TokenDTO
        {
            AccessToken = token,
            RefreshToken = refreshToken
        };
    }
}
