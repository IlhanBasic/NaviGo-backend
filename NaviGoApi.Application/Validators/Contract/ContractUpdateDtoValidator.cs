using FluentValidation;
using NaviGoApi.Application.DTOs.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.Validators.Contract
{
	public class ContractUpdateDtoValidator : AbstractValidator<ContractUpdateDto>
	{
		public ContractUpdateDtoValidator()
		{
			RuleFor(x => x.Terms)
				.MaximumLength(2000).When(x => x.Terms != null);

			RuleFor(x => x.PenaltyRatePerHour)
				.GreaterThanOrEqualTo(0).When(x => x.PenaltyRatePerHour.HasValue);

			RuleFor(x => x.MaxPenaltyPercent)
				.InclusiveBetween(0, 100).When(x => x.MaxPenaltyPercent.HasValue);

		}
	}
}
