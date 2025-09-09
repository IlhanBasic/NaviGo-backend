using FluentValidation;
using NaviGoApi.Application.DTOs.Contract;
using NaviGoApi.Domain.Entities;

namespace NaviGoApi.Application.Validators.Contract
{
	public class CarrierContractStatusUpdateDtoValidator : AbstractValidator<CarrierContractStatusUpdateDto>
	{
		public CarrierContractStatusUpdateDtoValidator()
		{
			When(x => x.ContractStatus == ContractStatus.Active, () =>
			{
				RuleFor(x => x.DriverId)
					.NotNull().WithMessage("DriverId is required when contract is Active.");

				RuleFor(x => x.VehicleId)
					.NotNull().WithMessage("VehicleId is required when contract is Active.");
			});

			When(x => x.ContractStatus == ContractStatus.Cancelled, () =>
			{
				RuleFor(x => x.DriverId)
					.Null().WithMessage("DriverId must not be set when contract is Cancelled.");

				RuleFor(x => x.VehicleId)
					.Null().WithMessage("VehicleId must not be set when contract is Cancelled.");
			});
		}
	}
}
