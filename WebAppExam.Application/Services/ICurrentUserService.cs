using System;

namespace WebAppExam.Application.Services;

public interface ICurrentUserService
{
    string UserId { get; }
    string Username { get; }
}
