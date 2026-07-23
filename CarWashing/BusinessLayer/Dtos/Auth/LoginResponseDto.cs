namespace BusinessLayer.Dtos.Auth
{
    public class LoginResponseDto
    {
        public Guid UserID { get; set; }
        public string Username { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string AccessToken { get; set; } = null!;
        public DateTime AccessTokenExpiration { get; set; }
    }
}
