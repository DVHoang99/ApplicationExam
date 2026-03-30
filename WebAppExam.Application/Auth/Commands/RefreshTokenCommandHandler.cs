using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WebAppExam.Application.Auth.DTOs;
using WebAppExam.Application.Auth.Services;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Auth.Commands;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, TokenDTO>
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly IAuthService _authService;


    public RefreshTokenCommandHandler(IUserRepository userRepository, IConfiguration configuration, IAuthService authService)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _authService = authService;
    }

    public async Task<TokenDTO> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var principal = GetPrincipalFromExpiredToken(request.Token);
        if (principal == null)
        {
            var failure = new FluentValidation.Results.ValidationFailure("RefreshToken", "Invalid access token or refresh token");
            throw new FluentValidation.ValidationException(new[] { failure });
        }

        string? username = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(username))
        {
            var failure = new FluentValidation.Results.ValidationFailure("username", "Username not found");
            throw new FluentValidation.ValidationException(new[] { failure });
        }

        // Find user and validate the refresh token
        var user = await _userRepository.GetByUsernameAsync(username, ct);

        if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
        {
            var failure = new FluentValidation.Results.ValidationFailure("RefreshToken", "Invalid access token or refresh token");
            throw new FluentValidation.ValidationException(new[] { failure });
        }

        var newAccessToken = _authService.GenerateJwtToken(user.Username, user.Role);
        var newRefreshToken = _authService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;

        return new TokenDTO
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        };
    }

    // Helper method to extract claims from an expired token
    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidIssuer = _configuration["Jwt:Issuer"],
            ValidAudience = _configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)),
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

        // Ensure the token algorithm is what we expect
        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }
}
