using FluentValidation;
using NaviGoApi.Application.DTOs.ShipmentStatusHistory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.Validators.ShipmentStatusHistory
{
	public class ShipmentStatusHistoryUpdateDtoValidator : AbstractValidator<ShipmentStatusHistoryUpdateDto>
	{
		public ShipmentStatusHistoryUpdateDtoValidator()
		{
			RuleFor(x => x.ShipmentStatus)
				.IsInEnum().WithMessage("Invalid shipment status.");
			RuleFor(x => x.Notes)
				.MaximumLength(500).WithMessage("Notes cannot exceed 500 characters.")
				.When(x => !string.IsNullOrEmpty(x.Notes));
		}
	}

}
