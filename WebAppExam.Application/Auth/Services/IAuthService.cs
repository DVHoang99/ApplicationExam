using System;
using FluentResults;
using WebAppExam.Application.Auth.DTOs;

namespace WebAppExam.Application.Auth.Services;

public interface IAuthService
{
    string GenerateJwtToken(string username, string role);
    string GenerateRefreshToken();
    Task<Result<TokenDTO>> RefreshToken(string token, string refreshToken, CancellationToken cancellationToken = default);
    Task<Result<TokenDTO>> Login(string username, string password, CancellationToken cancellationToken = default);
    Task<Result<string>> Logout(string username, CancellationToken cancellationToken = default);
}
