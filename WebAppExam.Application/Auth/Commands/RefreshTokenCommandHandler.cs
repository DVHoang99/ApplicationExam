using FluentResults;
using MediatR;
using WebAppExam.Application.Auth.DTOs;
using WebAppExam.Application.Auth.Services;

namespace WebAppExam.Application.Auth.Commands;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<TokenDTO>>
{
    private readonly IAuthService _authService;


    public RefreshTokenCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result<TokenDTO>> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        return await _authService.RefreshToken(request.Token, request.RefreshToken, ct);
    }
}
