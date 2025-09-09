using FluentValidation;
using NaviGoApi.Application.DTOs.Shipment;

namespace NaviGoApi.Application.Validators.Shipment
{
	public class ClientShipmentCreateDtoValidator : AbstractValidator<ClientShipmentCreateDto>
	{
		public ClientShipmentCreateDtoValidator()
		{
			RuleFor(x => x.CargoTypeId)
				.GreaterThan(0).WithMessage("CargoTypeId must be greater than 0.");

			RuleFor(x => x.WeightKg)
				.GreaterThan(0).WithMessage("Weight must be greater than 0.");

			RuleFor(x => x.Priority)
				.GreaterThanOrEqualTo(0).WithMessage("Priority cannot be negative.");

			RuleFor(x => x.Description)
				.MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");

			RuleFor(x => x.ScheduledDeparture)
				.LessThan(x => x.ScheduledArrival)
				.WithMessage("ScheduledDeparture must be before ScheduledArrival.");

			RuleFor(x => x.ScheduledArrival)
				.GreaterThan(x => x.ScheduledDeparture)
				.WithMessage("ScheduledArrival must be after ScheduledDeparture.");
		}
	}
}
