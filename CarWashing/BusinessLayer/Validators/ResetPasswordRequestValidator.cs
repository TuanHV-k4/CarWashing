using BusinessLayer.Dtos.Auth; using FluentValidation;
namespace BusinessLayer.Validators;
public sealed class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequestDto> { public ResetPasswordRequestValidator() { RuleFor(x => x.Token).NotEmpty().MaximumLength(256); RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(6).MaximumLength(128).Matches("[A-Z]").Matches("[a-z]").Matches("[0-9]").Matches("[!@#$%^&*()\\-+]"); RuleFor(x => x.ConfirmPassword).Equal(x => x.NewPassword); } }
