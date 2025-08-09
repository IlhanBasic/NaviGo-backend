using FluentValidation;
using NaviGoApi.Application.DTOs.DelayPenalty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.Validators.DelayPenalty
{
	public class DelayPenaltyUpdateDtoValidator : AbstractValidator<DelayPenaltyUpdateDto>
	{
		public DelayPenaltyUpdateDtoValidator()
		{
			RuleFor(x => x.CalculatedAt)
				.NotEmpty().WithMessage("CalculatedAt is required.")
				.LessThanOrEqualTo(DateTime.UtcNow).WithMessage("CalculatedAt cannot be in the future.");

			RuleFor(x => x.DelayHours)
				.GreaterThanOrEqualTo(0).WithMessage("DelayHours must be zero or greater.");

			RuleFor(x => x.PenaltyAmount)
				.GreaterThanOrEqualTo(0).WithMessage("PenaltyAmount must be zero or greater.");

			RuleFor(x => x.DelayPenaltiesStatus)
				.IsInEnum().WithMessage("Invalid delay penalty status.");
		}
	}
}
