using FluentValidation;
using NaviGoApi.Application.DTOs.ShipmentStatusHistory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.Validators.ShipmentStatusHistory
{
	public class ShipmentStatusHistoryCreateDtoValidator : AbstractValidator<ShipmentStatusHistoryCreateDto>
	{
		public ShipmentStatusHistoryCreateDtoValidator()
		{
			RuleFor(x => x.ShipmentId)
				.GreaterThan(0).WithMessage("ShipmentId must be greater than 0.");

			RuleFor(x => x.ShipmentStatus)
				.IsInEnum().WithMessage("Invalid shipment status.");

			RuleFor(x => x.ChangedAt)
				.NotEmpty().WithMessage("ChangedAt must be provided.")
				.LessThanOrEqualTo(DateTime.UtcNow).WithMessage("ChangedAt cannot be in the future.");

			RuleFor(x => x.ChangedByUserId)
				.GreaterThan(0).WithMessage("ChangedByUserId must be greater than 0.");

			RuleFor(x => x.Notes)
				.MaximumLength(500).WithMessage("Notes cannot exceed 500 characters.")
				.When(x => !string.IsNullOrEmpty(x.Notes));
		}
	}
}
