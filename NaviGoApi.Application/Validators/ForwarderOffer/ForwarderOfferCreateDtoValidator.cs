using FluentValidation;
using NaviGoApi.Application.DTOs.ForwarderOffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.Validators.ForwarderOffer
{
	public class ForwarderOfferCreateDtoValidator : AbstractValidator<ForwarderOfferCreateDto>
	{
		public ForwarderOfferCreateDtoValidator()
		{
			RuleFor(x => x.RouteId)
				.GreaterThan(0).WithMessage("RouteId must be greater than zero.");

			RuleFor(x => x.ForwarderId)
				.GreaterThan(0).WithMessage("ForwarderId must be greater than zero.");

			RuleFor(x => x.CommissionRate)
				.InclusiveBetween(0, 100).WithMessage("CommissionRate must be between 0 and 100.");

			RuleFor(x => x.ExpiresAt)
				.GreaterThan(DateTime.UtcNow).WithMessage("ExpiresAt must be a future date.");

			RuleFor(x => x.DiscountRate)
				.InclusiveBetween(0, 100).WithMessage("DiscountRate must be between 0 and 100.");
		}
	}
}
