using System;
using FluentResults;
using WebAppExam.Application.Auth.DTOs;

namespace WebAppExam.Application.Auth.Services;

/// <summary>
/// Defines the contract for authentication and authorization operations.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Generates a JSON Web Token (JWT) for a user.
    /// </summary>
    /// <param name="username">The username of the user.</param>
    /// <param name="role">The role of the user.</param>
    /// <returns>A string representing the JWT token.</returns>
    string GenerateJwtToken(string username, string role);

    /// <summary>
    /// Generates a new refresh token.
    /// </summary>
    /// <returns>A string representing the refresh token.</returns>
    string GenerateRefreshToken();

    /// <summary>
    /// Refreshes an expired JWT token using a valid refresh token.
    /// </summary>
    /// <param name="token">The expired JWT token.</param>
    /// <param name="refreshToken">The refresh token.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A data transfer object containing the new JWT and refresh tokens.</returns>
    Task<Result<TokenDTO>> RefreshToken(string token, string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates a user and returns a set of tokens.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <param name="password">The password.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A data transfer object containing the JWT and refresh tokens.</returns>
    Task<Result<TokenDTO>> Login(string username, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs out a user.
    /// </summary>
    /// <param name="username">The username of the user to log out.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A success or failure message.</returns>
    Task<Result<string>> Logout(string username, CancellationToken cancellationToken = default);
}
