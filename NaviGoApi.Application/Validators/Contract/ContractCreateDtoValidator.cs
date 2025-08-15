using FluentValidation;
using NaviGoApi.Application.DTOs.Contract;

namespace NaviGoApi.Application.Validators.Contract
{
	public class ContractCreateDtoValidator : AbstractValidator<ContractCreateDto>
	{
		public ContractCreateDtoValidator()
		{
			RuleFor(x => x.ClientId)
				.GreaterThan(0).WithMessage("ClientId is required.");

			RuleFor(x => x.ForwarderId)
				.GreaterThan(0).WithMessage("ForwarderId is required.");

			RuleFor(x => x.RouteId)
				.GreaterThan(0).WithMessage("RouteId is required.");

			RuleFor(x => x.RoutePriceId)
				.GreaterThan(0).WithMessage("RoutePriceId is required."); 

			RuleFor(x => x.ForwarderOfferId)
				.GreaterThan(0).WithMessage("ForwarderOfferId is required.");

			RuleFor(x => x.ContractNumber)
				.NotEmpty().WithMessage("ContractNumber is required.")
				.MaximumLength(50);

			RuleFor(x => x.Terms)
				.NotEmpty().WithMessage("Terms are required.");

			RuleFor(x => x.PenaltyRatePerHour)
				.GreaterThanOrEqualTo(0).WithMessage("PenaltyRatePerHour cannot be negative.");

			RuleFor(x => x.MaxPenaltyPercent)
				.InclusiveBetween(0, 100).WithMessage("MaxPenaltyPercent must be between 0 and 100.");
		}
	}
}
