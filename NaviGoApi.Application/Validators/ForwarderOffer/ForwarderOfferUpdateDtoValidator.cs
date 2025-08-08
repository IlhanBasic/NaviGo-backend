using FluentValidation;
using NaviGoApi.Application.DTOs.ForwarderOffer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.Validators.ForwarderOffer
{
	public class ForwarderOfferUpdateDtoValidator : AbstractValidator<ForwarderOfferUpdateDto>
	{
		public ForwarderOfferUpdateDtoValidator()
		{
			RuleFor(x => x.CommissionRate)
				.InclusiveBetween(0, 100)
				.When(x => x.CommissionRate.HasValue)
				.WithMessage("CommissionRate must be between 0 and 100.");

			RuleFor(x => x.ForwarderOfferStatus)
				.IsInEnum()
				.When(x => x.ForwarderOfferStatus.HasValue)
				.WithMessage("Invalid ForwarderOfferStatus.");

			RuleFor(x => x.RejectionReason)
				.NotEmpty()
				.When(x => x.ForwarderOfferStatus == Domain.Entities.ForwarderOfferStatus.Rejected)
				.WithMessage("RejectionReason is required when status is Rejected.");

			RuleFor(x => x.ExpiresAt)
				.GreaterThan(DateTime.UtcNow)
				.When(x => x.ExpiresAt.HasValue)
				.WithMessage("ExpiresAt must be a future date.");

			RuleFor(x => x.DiscountRate)
				.InclusiveBetween(0, 100)
				.When(x => x.DiscountRate.HasValue)
				.WithMessage("DiscountRate must be between 0 and 100.");
		}
	}
}
