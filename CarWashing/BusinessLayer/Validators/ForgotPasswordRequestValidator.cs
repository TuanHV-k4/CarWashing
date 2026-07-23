using BusinessLayer.Dtos.Auth; using FluentValidation;
namespace BusinessLayer.Validators;
public sealed class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequestDto> { public ForgotPasswordRequestValidator() { RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200); } }
