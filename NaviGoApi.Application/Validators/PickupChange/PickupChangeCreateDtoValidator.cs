using FluentValidation;
using NaviGoApi.Application.DTOs.PickupChange;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.Validators.PickupChange
{
	public class PickupChangeCreateDtoValidator : AbstractValidator<PickupChangeCreateDto>
	{
		public PickupChangeCreateDtoValidator()
		{
			RuleFor(x => x.ShipmentId).GreaterThan(0);
			RuleFor(x => x.NewTime).GreaterThan(DateTime.UtcNow).WithMessage("NewTime cannot be in past.");
		}
	}
}
