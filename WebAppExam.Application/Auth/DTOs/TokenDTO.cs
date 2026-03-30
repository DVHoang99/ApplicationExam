using System;

namespace WebAppExam.Application.Auth.DTOs;

public class TokenDTO
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}
