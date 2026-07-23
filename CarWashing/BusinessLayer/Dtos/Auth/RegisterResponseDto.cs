namespace BusinessLayer.Dtos.Auth
{
    public class RegisterResponseDto
    {
        public Guid UserID { get; set; }
        public string Username { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
