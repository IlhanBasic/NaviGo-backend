using FluentValidation;
using NaviGoApi.Application.DTOs.Driver;
using System;

namespace NaviGoApi.Application.Validators.Driver
{
	public class DriverCreateDtoValidator : AbstractValidator<DriverCreateDto>
	{
		public DriverCreateDtoValidator()
		{
			RuleFor(x => x.CompanyId)
				.GreaterThan(0).WithMessage("CompanyId must be greater than 0.");

			RuleFor(x => x.FirstName)
				.NotEmpty().WithMessage("FirstName is required.")
				.MaximumLength(50).WithMessage("FirstName max length is 50.");

			RuleFor(x => x.LastName)
				.NotEmpty().WithMessage("LastName is required.")
				.MaximumLength(50).WithMessage("LastName max length is 50.");

			RuleFor(x => x.PhoneNumber)
				.NotEmpty().WithMessage("PhoneNumber is required.")
				.MaximumLength(20).WithMessage("PhoneNumber max length is 20.");

			RuleFor(x => x.LicenseNumber)
				.NotEmpty().WithMessage("LicenseNumber is required.")
				.MaximumLength(20).WithMessage("LicenseNumber max length is 20.");

			RuleFor(x => x.LicenseExpiry)
				.Must(date => date == null || date > DateTime.Now)
				.WithMessage("LicenseExpiry must be in the future or null.");

			RuleFor(x => x.LicenseCategories)
				.NotEmpty().WithMessage("LicenseCategories is required.");

			RuleFor(x => x.HireDate)
				.LessThanOrEqualTo(DateTime.Today)
				.WithMessage("HireDate cannot be in the future.");

			RuleFor(x => x.DriverStatus)
				.IsInEnum()
				.WithMessage("Invalid DriverStatus value.");
		}
	}
}
