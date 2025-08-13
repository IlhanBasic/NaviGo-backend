using FluentValidation;
using NaviGoApi.Application.DTOs.VehicleMaintenance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.Validators.VehicleMaintenance
{
	public class VehicleMaintenanceCreateDtoValidator : AbstractValidator<VehicleMaintenanceCreateDto>
	{
		public VehicleMaintenanceCreateDtoValidator()
		{
			RuleFor(x => x.VehicleId)
				.GreaterThan(0).WithMessage("VehicleId must be greater than 0.");

			RuleFor(x => x.Description)
				.NotEmpty().WithMessage("Description is required.")
				.MaximumLength(1000).WithMessage("Description can't be longer than 1000 characters.");
			RuleFor(x => x.Severity)
				.IsInEnum().WithMessage("Invalid severity level.");

			RuleFor(x => x.MaintenanceType)
				.IsInEnum().WithMessage("Invalid maintenance type.");

			RuleFor(x => x.RepairCost)
				.GreaterThanOrEqualTo(0).When(x => x.RepairCost.HasValue)
				.WithMessage("Repair cost must be zero or positive.");
		}
	}
}
