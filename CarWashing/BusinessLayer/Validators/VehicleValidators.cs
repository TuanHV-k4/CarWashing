using BusinessLayer.Dtos.Vehicle;
using FluentValidation;

namespace BusinessLayer.Validators
{
    public class CreateVehicleRequestValidator : AbstractValidator<CreateVehicleRequestDto>
    {
        public CreateVehicleRequestValidator()
        {
            RuleFor(x => x.LicensePlate)
                .NotEmpty().WithMessage("License plate is required.")
                .MaximumLength(20);

            RuleFor(x => x.VehicleType)
                .IsInEnum()
                .When(x => x.VehicleType.HasValue);
            RuleFor(x => x.Brand).MaximumLength(100);
            RuleFor(x => x.Model).MaximumLength(100);
            RuleFor(x => x.Color).MaximumLength(50);
        }
    }

    public class UpdateVehicleRequestValidator : AbstractValidator<UpdateVehicleRequestDto>
    {
        public UpdateVehicleRequestValidator()
        {
            RuleFor(x => x.VehicleType)
                .IsInEnum()
                .When(x => x.VehicleType.HasValue);
            RuleFor(x => x.Brand).MaximumLength(100);
            RuleFor(x => x.Model).MaximumLength(100);
            RuleFor(x => x.Color).MaximumLength(50);
        }
    }
}
