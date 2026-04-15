using System;
using FluentResults;

namespace WebAppExam.Application.User.Services;

/// <summary>
/// Defines the contract for user-related operations.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="username">The unique username for the user.</param>
    /// <param name="password">The user's password.</param>
    /// <param name="role">The role assigned to the user.</param>
    /// <param name="name">The user's full name.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The unique identifier of the created user.</returns>
    Task<Result<Ulid>> CreateUserAsync(string username, string password, string role, string name, CancellationToken cancellationToken = default);
}
