using System;

namespace WebAppExam.Application.Auth.DTOs;

public class UserLoginDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
