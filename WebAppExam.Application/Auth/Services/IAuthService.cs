using System;

namespace WebAppExam.Application.Auth.Services;

public interface IAuthService
{
    string GenerateJwtToken(string username, string role);
    string GenerateRefreshToken();
}
