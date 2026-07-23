using BusinessLayer.Dtos.Auth;
using FluentValidation;

namespace BusinessLayer.Validators
{
    public class ResendOtpRequestValidator : AbstractValidator<ResendOtpRequestDto>
    {
        public ResendOtpRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.");
        }
    }
}
