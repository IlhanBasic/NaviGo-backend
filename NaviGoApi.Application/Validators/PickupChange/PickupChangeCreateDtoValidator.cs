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
			RuleFor(x => x.ClientId).GreaterThan(0);
			RuleFor(x => x.OldTime).LessThan(x => x.NewTime).WithMessage("OldTime must be before NewTime");
			RuleFor(x => x.ChangeCount).GreaterThanOrEqualTo(0);
			RuleFor(x => x.AdditionalFee).GreaterThanOrEqualTo(0);
			RuleFor(x => x.PickupChangesStatus).InclusiveBetween(0, 10); // prilagodi prema statusima koje imaš
		}
	}
}
