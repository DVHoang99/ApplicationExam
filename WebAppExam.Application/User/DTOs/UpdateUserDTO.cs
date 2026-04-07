namespace WebAppExam.Application.User.DTOs
{
    public class UpdateUserDTO
    {
        public string Username { get; private set; }
        public string Name { get; private set; }
        public string Role { get; private set; }
        public string Password { get; private set; }
    }
}