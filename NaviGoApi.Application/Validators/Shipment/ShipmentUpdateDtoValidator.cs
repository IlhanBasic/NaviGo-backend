using FluentValidation;
using NaviGoApi.Application.DTOs.Shipment;
using System;

namespace NaviGoApi.Application.Validators.Shipment
{
	public class ShipmentUpdateDtoValidator : AbstractValidator<ShipmentUpdateDto>
	{
		public ShipmentUpdateDtoValidator()
		{
			RuleFor(x => x.Status)
				.IsInEnum()
				.WithMessage("Invalid shipment status.");

			RuleFor(x => x.Description)
				.MaximumLength(500)
				.WithMessage("Description cannot be longer than 500 characters.");

			RuleFor(x => x.ActualDeparture)
				.LessThan(x => x.ActualArrival)
				.When(x => x.ActualDeparture.HasValue && x.ActualArrival.HasValue)
				.WithMessage("Actual departure must be before actual arrival.");

			RuleFor(x => x.ActualArrival)
				.GreaterThanOrEqualTo(x => x.ActualDeparture ?? DateTime.MinValue)
				.When(x => x.ActualArrival.HasValue)
				.WithMessage("Actual arrival must be after or equal to actual departure.");

			// 🔹 Obe vrednosti moraju biti <= DateTime.Now
			RuleFor(x => x.ActualDeparture)
				.LessThanOrEqualTo(DateTime.Now)
				.When(x => x.ActualDeparture.HasValue)
				.WithMessage("Actual departure cannot be in the future.");

			RuleFor(x => x.ActualArrival)
				.LessThanOrEqualTo(DateTime.Now)
				.When(x => x.ActualArrival.HasValue)
				.WithMessage("Actual arrival cannot be in the future.");
		}
	}
}
