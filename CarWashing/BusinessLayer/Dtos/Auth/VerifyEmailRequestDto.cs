namespace BusinessLayer.Dtos.Auth
{
    public class VerifyEmailRequestDto
    {
        public string Email { get; set; } = null!;
        public string OtpCode { get; set; } = null!;
    }
}
