using FluentValidation;
using NaviGoApi.Application.DTOs.Shipment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.Validators.Shipment
{
	public class ShipmentUpdateDtoValidator : AbstractValidator<ShipmentUpdateDto>
	{
		public ShipmentUpdateDtoValidator()
		{

			RuleFor(x => x.ContractId)
				.GreaterThan(0).WithMessage("ContractId must be greater than 0.");

			RuleFor(x => x.VehicleId)
				.GreaterThan(0).WithMessage("VehicleId must be greater than 0.");

			RuleFor(x => x.DriverId)
				.GreaterThan(0).WithMessage("DriverId must be greater than 0.");

			RuleFor(x => x.CargoTypeId)
				.GreaterThan(0).WithMessage("CargoTypeId must be greater than 0.");

			RuleFor(x => x.WeightKg)
				.GreaterThan(0).WithMessage("Weight must be greater than 0.");

			RuleFor(x => x.Priority)
				.InclusiveBetween(1, 5).WithMessage("Priority must be between 1 and 5.");

			RuleFor(x => x.Status)
				.IsInEnum().WithMessage("Invalid shipment status.");

			RuleFor(x => x.ScheduledDeparture)
				.LessThan(x => x.ScheduledArrival)
				.WithMessage("Scheduled departure must be before scheduled arrival.");

			RuleFor(x => x.DelayHours)
				.GreaterThanOrEqualTo(0).When(x => x.DelayHours.HasValue);

			RuleFor(x => x.DelayPenaltyAmount)
				.GreaterThanOrEqualTo(0).When(x => x.DelayPenaltyAmount.HasValue);
		}
	}
}
