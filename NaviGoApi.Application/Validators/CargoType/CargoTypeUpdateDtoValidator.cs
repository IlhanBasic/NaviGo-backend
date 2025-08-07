using FluentValidation;
using NaviGoApi.Application.DTOs.CargoType;

namespace NaviGoApi.Application.Validators.CargoType
{
	public class CargoTypeUpdateDtoValidator : AbstractValidator<CargoTypeUpdateDto>
	{
		public CargoTypeUpdateDtoValidator()
		{
			RuleFor(x => x.TypeName)
				.NotEmpty().WithMessage("TypeName is required.")
				.MaximumLength(100).WithMessage("TypeName must not exceed 100 characters.");

			RuleFor(x => x.Description)
				.MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
				.When(x => x.Description != null);

			RuleFor(x => x.HazardLevel)
				.GreaterThanOrEqualTo(0).WithMessage("HazardLevel must be zero or greater.");

			RuleFor(x => x.RequiresSpecialEquipment)
				.NotNull().WithMessage("RequiresSpecialEquipment is required.");
		}
	}
}
