using System;
using Microsoft.AspNetCore.StaticAssets;
using WebAppExam.Application.Auth.DTOs;
using WebAppExam.Application.Shared;

namespace WebAppExam.Application.Auth.Commands;

public class RefreshTokenCommand : ICommand<TokenDTO>
{
    public required string Token { get; set; }
    public required string RefreshToken { get; set; }
    public static RefreshTokenCommand Refresh(string token, string refreshToken)
    {
        return new RefreshTokenCommand
        {
            Token = token,
            RefreshToken = refreshToken
        };
    }
}
