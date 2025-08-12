using FluentValidation;
using NaviGoApi.Application.DTOs.DelayPenalty;

namespace NaviGoApi.Application.Validators.DelayPenalty
{
	public class DelayPenaltyCreateDtoValidator : AbstractValidator<DelayPenaltyCreateDto>
	{
		public DelayPenaltyCreateDtoValidator()
		{
			RuleFor(x => x.ShipmentId)
				.GreaterThan(0).WithMessage("ShipmentId must be greater than 0.");

			RuleFor(x => x.DelayHours)
				.GreaterThanOrEqualTo(0).WithMessage("DelayHours must be zero or greater.");

			RuleFor(x => x.PenaltyAmount)
				.GreaterThanOrEqualTo(0).WithMessage("PenaltyAmount must be zero or greater.");

			RuleFor(x => x.DelayPenaltiesStatus)
				.IsInEnum().WithMessage("Invalid delay penalty status.");
		}
	}
}
