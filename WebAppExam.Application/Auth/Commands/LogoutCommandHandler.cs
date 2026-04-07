using System;
using FluentResults;
using MediatR;
using WebAppExam.Application.Auth.Services;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Auth.Commands;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result<string>>
{
    private readonly IAuthService _authService;


    public LogoutCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Result<string>> Handle(LogoutCommand request, CancellationToken ct)
    {
        return await _authService.Logout(request.Username, ct);
    }
}
