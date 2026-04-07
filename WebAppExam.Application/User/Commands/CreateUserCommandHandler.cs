using System;
using FluentResults;
using MediatR;
using WebAppExam.Application.Common.Helpers;
using WebAppExam.Application.User.Services;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.User.Commands;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<Ulid>>
{
    private readonly IUserService _userService;

    public CreateUserCommandHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<Result<Ulid>> Handle(CreateUserCommand request, CancellationToken ct)
    {
        return await _userService.CreateUserAsync(request.Username, request.Password, request.Role, request.Name, ct);
    }
}
