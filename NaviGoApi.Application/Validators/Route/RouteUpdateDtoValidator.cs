using FluentValidation;
using NaviGoApi.Application.DTOs.Route;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.Validators.Route
{
	public class RouteUpdateDtoValidator : AbstractValidator<RouteUpdateDto>
	{
		public RouteUpdateDtoValidator()
		{

			RuleFor(x => x.CompanyId)
				.GreaterThan(0).WithMessage("CompanyId must be greater than 0.");

			RuleFor(x => x.StartLocationId)
				.GreaterThan(0).WithMessage("StartLocationId must be greater than 0.");

			RuleFor(x => x.EndLocationId)
				.GreaterThan(0).WithMessage("EndLocationId must be greater than 0.")
				.NotEqual(x => x.StartLocationId).WithMessage("EndLocationId must be different from StartLocationId.");

			RuleFor(x => x.BasePrice)
				.GreaterThan(0).WithMessage("BasePrice must be greater than 0.");

			RuleFor(x => x.AvailableFrom)
				.LessThan(x => x.AvailableTo).WithMessage("AvailableFrom must be earlier than AvailableTo.");

			RuleFor(x => x.AvailableTo)
				.GreaterThan(x => x.AvailableFrom).WithMessage("AvailableTo must be later than AvailableFrom.");
		}
	}
}
