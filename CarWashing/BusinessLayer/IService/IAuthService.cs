using BusinessLayer.Dtos.Auth;

namespace BusinessLayer.IService
{
    public interface IAuthService
    {
        Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request);
        Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
        Task VerifyEmailAsync(VerifyEmailRequestDto request);
        Task ResendOtpAsync(ResendOtpRequestDto request);
        Task RequestPasswordResetAsync(ForgotPasswordRequestDto request);
        Task ResetPasswordAsync(ResetPasswordRequestDto request);
    }
}
