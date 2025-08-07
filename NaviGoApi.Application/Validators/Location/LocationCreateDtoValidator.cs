using FluentValidation;
using NaviGoApi.Application.DTOs.Location;

namespace NaviGoApi.Application.Validators.Location
{
	public class LocationCreateDtoValidator : AbstractValidator<LocationCreateDto>
	{
		public LocationCreateDtoValidator()
		{
			RuleFor(x => x.City)
				.NotEmpty().WithMessage("City is required.")
				.MaximumLength(100);

			RuleFor(x => x.Country)
				.NotEmpty().WithMessage("Country is required.")
				.MaximumLength(100);

			RuleFor(x => x.ZIP)
				.NotEmpty().WithMessage("ZIP code is required.")
				.MaximumLength(20);

			RuleFor(x => x.Latitude)
				.InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90.");

			RuleFor(x => x.Longitude)
				.InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180.");

			RuleFor(x => x.FullAddress)
				.NotEmpty().WithMessage("Full address is required.")
				.MaximumLength(255);
		}
	}
}
