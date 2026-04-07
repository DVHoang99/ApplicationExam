using System;
using FluentResults;
using Microsoft.AspNetCore.StaticAssets;
using WebAppExam.Application.Auth.DTOs;
using WebAppExam.Application.Shared;

namespace WebAppExam.Application.Auth.Commands;

public class RefreshTokenCommand(string token, string refreshToken) : ICommand<Result<TokenDTO>>
{
    public string Token { get; private set; } = token;
    public string RefreshToken { get; private set; } = refreshToken;
    public static RefreshTokenCommand Refresh(string token, string refreshToken)
    {
        return new RefreshTokenCommand(token, refreshToken);
    }
}
