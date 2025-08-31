using FluentValidation;
using NaviGoApi.Application.DTOs.Vehicle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.Validators.Vehicle
{
	public class VehicleUpdateDtoValidator : AbstractValidator<VehicleUpdateDto>
	{
		public VehicleUpdateDtoValidator()
		{

			RuleFor(v => v.CompanyId)
				.GreaterThan(0).WithMessage("CompanyId must be greater than zero.");

			RuleFor(v => v.VehicleTypeId)
				.GreaterThan(0).WithMessage("VehicleTypeId must be greater than zero.");

			RuleFor(v => v.CapacityKg)
				.GreaterThan(0).WithMessage("CapacityKg must be greater than zero.");

			RuleFor(v => v.ManufactureYear)
				.InclusiveBetween(1900, DateTime.UtcNow.Year)
				.WithMessage($"ManufactureYear must be between 1900 and {DateTime.UtcNow.Year}.");

			RuleFor(v => v.VehicleStatus)
				.IsInEnum().WithMessage("Invalid VehicleStatus.");

			RuleFor(v => v.LastInspectionDate)
				.LessThanOrEqualTo(DateTime.UtcNow)
				.When(v => v.LastInspectionDate.HasValue)
				.WithMessage("LastInspectionDate cannot be in the future.");

			RuleFor(v => v.InsuranceExpiry)
				.GreaterThan(DateTime.UtcNow)
				.When(v => v.InsuranceExpiry.HasValue)
				.WithMessage("InsuranceExpiry must be in the future.");

			RuleFor(v => v.Categories)
				.MaximumLength(200)
				.WithMessage("Categories can be at most 200 characters.");
			RuleFor(v => v)
				.Must(v => !v.LastInspectionDate.HasValue || v.LastInspectionDate.Value.Year >= v.ManufactureYear)
				.WithMessage("Last Inspection Date cannot be before Manufacture Year.");

		}
	}
}
