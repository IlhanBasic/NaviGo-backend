using FluentValidation;
using NaviGoApi.Application.DTOs.Contract;
using NaviGoApi.Application.Validators.Shipment;

namespace NaviGoApi.Application.Validators.Contract
{
	public class ClientContractCreateDtoValidator : AbstractValidator<ClientContractCreateDto>
	{
		public ClientContractCreateDtoValidator()
		{
			RuleFor(x => x.RoutePriceId)
				.GreaterThan(0).WithMessage("RoutePriceId is required and must be greater than 0.");

			RuleFor(x => x.ForwarderOfferId)
				.GreaterThan(0).WithMessage("ForwarderOfferId is required and must be greater than 0.");

			RuleFor(x => x.PenaltyRatePerHour)
				.GreaterThanOrEqualTo(0).WithMessage("PenaltyRatePerHour cannot be negative.");

			RuleFor(x => x.MaxPenaltyPercent)
				.GreaterThanOrEqualTo(0).WithMessage("MaxPenaltyPercent cannot be negative.");

			RuleFor(x => x.Shipments)
				.NotEmpty().WithMessage("At least one shipment must be provided.");
			
			RuleForEach(x => x.Shipments)
				.SetValidator(new ClientShipmentCreateDtoValidator());
		}
	}
}
