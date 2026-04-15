using System;

namespace WebAppExam.Application.Services;

/// <summary>
/// Defines the contract for accessing information about the currently authenticated user.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the unique identifier of the current user.
    /// </summary>
    string UserId { get; }

    /// <summary>
    /// Gets the username of the current user.
    /// </summary>
    string Username { get; }
}
