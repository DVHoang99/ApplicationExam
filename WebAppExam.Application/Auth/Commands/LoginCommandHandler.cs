using FluentResults;
using MediatR;
using WebAppExam.Application.Auth.DTOs;
using WebAppExam.Application.Auth.Services;
using WebAppExam.Application.Common.Helpers;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Auth.Commands;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<TokenDTO>>
{
    private readonly IAuthService _authService;

    public LoginCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result<TokenDTO>> Handle(LoginCommand request, CancellationToken ct)
    {
        return await _authService.Login(request.Username, request.Password, ct);
    }
}
