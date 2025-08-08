using FluentValidation;
using NaviGoApi.Application.DTOs.VehicleType;

namespace NaviGoApi.Application.Validators.VehicleType
{
	public class VehicleTypeUpdateDtoValidator : AbstractValidator<VehicleTypeUpdateDto>
	{
		public VehicleTypeUpdateDtoValidator()
		{

			RuleFor(x => x.TypeName)
				.NotEmpty()
				.WithMessage("TypeName is required.")
				.MaximumLength(100)
				.WithMessage("TypeName max length is 100 characters.");

			RuleFor(x => x.Description)
				.MaximumLength(500)
				.WithMessage("Description max length is 500 characters.")
				.When(x => !string.IsNullOrEmpty(x.Description));

			RuleFor(x => x.RequiresSpecialLicense)
				.NotNull()
				.WithMessage("RequiresSpecialLicense must be specified.");
		}
	}
}
