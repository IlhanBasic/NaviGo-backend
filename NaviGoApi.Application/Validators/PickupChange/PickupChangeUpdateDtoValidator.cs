using FluentValidation;
using NaviGoApi.Application.DTOs.PickupChange;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.Validators.PickupChange
{
	public class PickupChangeUpdateDtoValidator : AbstractValidator<PickupChangeUpdateDto>
	{
		public PickupChangeUpdateDtoValidator()
		{
			RuleFor(x => x.NewTime).GreaterThan(DateTime.UtcNow).WithMessage("NewTime cannot be in past.");
		}
	}
}
