using System;
using FluentResults;

namespace WebAppExam.Application.User.Services;

public interface IUserService
{
    Task<Result<Ulid>> CreateUserAsync(string username, string password, string role, string name, CancellationToken cancellationToken = default);
}
