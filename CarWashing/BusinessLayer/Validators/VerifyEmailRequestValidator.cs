using BusinessLayer.Dtos.Auth;
using FluentValidation;

namespace BusinessLayer.Validators
{
    public class VerifyEmailRequestValidator : AbstractValidator<VerifyEmailRequestDto>
    {
        public VerifyEmailRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.");

            RuleFor(x => x.OtpCode)
                .NotEmpty().WithMessage("OTP code is required.")
                .Length(6).WithMessage("OTP code must be exactly 6 digits.")
                .Matches(@"^\d{6}$").WithMessage("OTP code must contain only digits.");
        }
    }
}
