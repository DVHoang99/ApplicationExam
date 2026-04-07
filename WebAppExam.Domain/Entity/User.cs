using System;

namespace WebAppExam.Domain.Entity;

public class User : EntityBase
{
    public string Username { get; private set; }
    public string PasswordHash { get; private set; }
    public string Name { get; private set; }
    public string Role { get; private set; }
    public string? RefreshToken { get; private set; }
    public DateTime RefreshTokenExpiryTime { get; private set; }

    private User(string username, string passwordHash, string name, string role)
    {
        Username = username;
        PasswordHash = passwordHash;
        Name = name;
        Role = role;
        CreatedAt = DateTime.UtcNow;
    }

    public static User Create(string username, string passwordHash, string name, string role)
    {
        return new User(username, passwordHash, name, role);
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
