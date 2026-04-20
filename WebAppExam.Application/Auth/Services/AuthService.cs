using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FluentResults;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using WebAppExam.Application.Auth.DTOs;
using WebAppExam.Application.Common.Errors;
using WebAppExam.Application.Common.Helpers;
using WebAppExam.Domain.Exceptions;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Auth.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;


    public AuthService(IConfiguration configuration, IUserRepository userRepository)
    {
        _configuration = configuration;
        _userRepository = userRepository;
    }
    public string GenerateJwtToken(string username, string role)
    {

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
                new Claim(ClaimTypes.NameIdentifier, username),
                new Claim(ClaimTypes.Role, role)
            };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public async Task<Result<TokenDTO>> RefreshToken(string token, string refreshToken, CancellationToken ct)
    {
        var principal = GetPrincipalFromExpiredToken(token);
        if (principal == null)
        {
            throw new BadRequestException("Invalid access token or refresh token");
        }

        string? username = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(username))
        {
            throw new BadRequestException("Invalid access token or refresh token");
        }

        // Find user and validate the refresh token
        var user = await _userRepository.GetByUsernameAsync(username, ct);

        if (user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
        {
            throw new BadRequestException("Invalid access token or refresh token");
        }

        var newAccessToken = GenerateJwtToken(user.Username, user.Role);
        var newRefreshToken = GenerateRefreshToken();

        user.UpdateRefeshToken(user.RefreshToken, user.RefreshTokenExpiryTime);

        return Result.Ok(TokenDTO.FromResult(newAccessToken, newRefreshToken));
    }

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
            return null;
        }

        return principal;
    }

    public async Task<Result<TokenDTO>> Login(string username, string password, CancellationToken ct)
    {
        var user = await _userRepository.GetByUsernameAsync(username, ct);

        if (user == null || !PasswordHelper.VerifyPassword(password, user.PasswordHash))
        {
            throw new BadRequestException("Invalid username or password");
        }

        var token = GenerateJwtToken(user.Username, user.Role);
        var refreshToken = GenerateRefreshToken();

        user.UpdateRefeshToken(refreshToken, DateTime.UtcNow.AddDays(7));

        _userRepository.Update(user);

        return TokenDTO.FromResult(token, refreshToken);
    }

    public async Task<Result<string>> Logout(string username, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByUsernameAsync(username, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("User not found");
        }

        user.UpdateRefeshToken(string.Empty, DateTime.MinValue);

        _userRepository.Update(user);
        return Result.Ok(username);
    }
}
