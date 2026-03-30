using System;

namespace WebAppExam.Domain.Entity;

public class User : EntityBase
{
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public string? RefreshToken { get; set; }
    public DateTime RefreshTokenExpiryTime { get; set; }

    protected User() { }

    public User(string username, string passwordHash, string name, string role)
    {
        Username = username;
        PasswordHash = passwordHash;
        Name = name;
        Role = role;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateUser(string passwordHash, string name, string role)
    {
        UpdatedAt = DateTime.UtcNow;
        PasswordHash = passwordHash;
        Name = name;
        Role = role;
    }

    public void UpdateRefeshToken(string refreshToken, DateTime refreshTokenExpiryTime)
    {
        UpdatedAt = DateTime.UtcNow;
        RefreshToken = refreshToken;
        RefreshTokenExpiryTime = refreshTokenExpiryTime;
    }

    public void DeleteUser()
    {
        DeletedAt = DateTime.UtcNow;
    }

    
}
