using System;

namespace WebAppExam.Application.User.DTOs
{
    public class UserResponseDTO
    {
        public Ulid Id { get; private set; }
        public string Username { get; private set; }
        public string Name { get; private set; }
        public string Role { get; private set; }
        
        private UserResponseDTO()
        {
            Id = Ulid.Empty;
            Username = string.Empty;
            Name = string.Empty;
            Role = string.Empty;
        }

        public static UserResponseDTO FromResult(Domain.Entity.User? user)
        {
            if(user == null)
            {
                return new UserResponseDTO();
            }
            
            return new UserResponseDTO
            {
                Id = user.Id,
                Username = user.Username,
                Name = user.Name,
                Role = user.Role
            };
        }
    }
}