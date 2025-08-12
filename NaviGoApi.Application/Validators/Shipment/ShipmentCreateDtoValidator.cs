using FluentValidation;
using NaviGoApi.Application.DTOs.Shipment;
using System;

namespace NaviGoApi.Application.Validators.Shipment
{
	public class ShipmentCreateDtoValidator : AbstractValidator<ShipmentCreateDto>
	{
		public ShipmentCreateDtoValidator()
		{
			RuleFor(x => x.ContractId)
				.GreaterThan(0)
				.WithMessage("ContractId must be greater than 0.");

			RuleFor(x => x.VehicleId)
				.GreaterThan(0)
				.WithMessage("VehicleId must be greater than 0.");

			RuleFor(x => x.DriverId)
				.GreaterThan(0)
				.WithMessage("DriverId must be greater than 0.");

			RuleFor(x => x.CargoTypeId)
				.GreaterThan(0)
				.WithMessage("CargoTypeId must be greater than 0.");

			RuleFor(x => x.WeightKg)
				.GreaterThan(0)
				.WithMessage("Weight must be greater than 0.");

			RuleFor(x => x.Priority)
				.InclusiveBetween(1, 5)
				.WithMessage("Priority must be between 1 and 5.");

			RuleFor(x => x.ScheduledDeparture)
				.GreaterThan(DateTime.UtcNow)
				.WithMessage("Scheduled departure must be in the future.");

			RuleFor(x => x.ScheduledArrival)
				.GreaterThan(x => x.ScheduledDeparture)
				.WithMessage("Scheduled arrival must be after scheduled departure.");

			RuleFor(x => x.Description)
				.MaximumLength(500)
				.WithMessage("Description cannot be longer than 500 characters.");
		}
	}
}
