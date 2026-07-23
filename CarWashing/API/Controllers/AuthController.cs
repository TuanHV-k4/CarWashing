using System.Security.Claims;
using BusinessLayer.Dtos.Auth;
using BusinessLayer.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Register a new customer account.
        /// Email verification is disabled for local/demo flow.
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(RegisterResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            var result = await _authService.RegisterAsync(request);
            return CreatedAtAction(nameof(Register), new { id = result.UserID }, result);
        }

        /// <summary>
        /// No-op email verification endpoint for compatibility.
        /// </summary>
        [HttpPost("verify-email")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequestDto request)
        {
            await _authService.VerifyEmailAsync(request);
            return Ok(new { Message = "Email verification is disabled. Account is ready to log in." });
        }

        /// <summary>
        /// No-op resend OTP endpoint for compatibility.
        /// </summary>
        [HttpPost("resend-otp")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpRequestDto request)
        {
            await _authService.ResendOtpAsync(request);
            return Ok(new { Message = "Email verification is disabled. No OTP was sent." });
        }

        /// <summary>
        /// Authenticate and receive a JWT access token.
        /// Accepts username or email as the Username field.
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var result = await _authService.LoginAsync(request);
            return Ok(result);
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        [EnableRateLimiting("PasswordReset")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            await _authService.RequestPasswordResetAsync(request);
            return Ok(new { Message = "Nếu email đã được đăng ký, chúng tôi đã gửi hướng dẫn đặt lại mật khẩu." });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            await _authService.ResetPasswordAsync(request);
            return Ok(new { Message = "Mật khẩu đã được đặt lại. Vui lòng đăng nhập lại." });
        }

        /// <summary>
        /// Get current authenticated user's profile.
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public IActionResult GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var fullName = User.FindFirst("FullName")?.Value;

            return Ok(new
            {
                UserID = userId,
                Username = username,
                Email = email,
                FullName = fullName,
                Role = role
            });
        }

        /// <summary>
        /// Admin-only test endpoint.
        /// </summary>
        [HttpGet("admin-only")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult AdminOnly()
        {
            return Ok(new { Message = "Welcome, Admin! You have access to this endpoint." });
        }

        /// <summary>
        /// Customer-only test endpoint.
        /// </summary>
        [HttpGet("customer-only")]
        [Authorize(Policy = "CustomerOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult CustomerOnly()
        {
            return Ok(new { Message = "Welcome, Customer! You have access to this endpoint." });
        }
    }
}
