using System;
using FluentResults;
using WebAppExam.Application.Common.Helpers;
using WebAppExam.Domain.Exceptions;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.User.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<Ulid>> CreateUserAsync(string username, string password, string role, string name, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByUsernameAsync(username, cancellationToken);

        if (user != null)
        {
            throw new BadRequestException("Username already exists");
        }
        var newUser = Domain.Entity.User.Create(username, PasswordHelper.HashPassword(password), name, role);
        await _userRepository.AddAsync(newUser, cancellationToken);

        return newUser.Id;
    }
}
