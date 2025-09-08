using FluentValidation;
using NaviGoApi.Application.DTOs.RoutePrice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.Validators.RoutePrice
{
	public class RoutePriceCreateDtoValidator : AbstractValidator<RoutePriceCreateDto>
	{
		public RoutePriceCreateDtoValidator()
		{
			RuleFor(x => x.RouteId)
				.GreaterThan(0).WithMessage("RouteId must be greater than 0.");

			RuleFor(x => x.VehicleTypeId)
				.GreaterThan(0).WithMessage("VehicleTypeId must be greater than 0.");

			RuleFor(x => x.PricePerKm)
				.GreaterThanOrEqualTo(0).WithMessage("Price per km must be non-negative.");

			RuleFor(x => x.PricePerKg)
				.GreaterThanOrEqualTo(0).WithMessage("Price per km must be non-negative.");

			RuleFor(x => x.MinimumPrice)
				.GreaterThanOrEqualTo(0).WithMessage("Minimum price must be non-negative.");
		}
	}
}
