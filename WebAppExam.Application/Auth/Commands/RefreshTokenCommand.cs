using System;
using WebAppExam.Application.Auth.DTOs;
using WebAppExam.Application.Shared;

namespace WebAppExam.Application.Auth.Commands;

public class RefreshTokenCommand(string token, string refreshToken) : ICommand<TokenDTO>
{
    public string Token { get; set; } = token;
    public string RefreshToken { get; set; } = refreshToken;
}
