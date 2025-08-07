using FluentValidation;
using NaviGoApi.Application.DTOs.VehicleType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.Validators.VehicleType
{
	public class VehicleTypeCreateDtoValidator : AbstractValidator<VehicleTypeCreateDto>
	{
		public VehicleTypeCreateDtoValidator()
		{
			RuleFor(x => x.TypeName)
				.NotEmpty().WithMessage("TypeName is required.")
				.MaximumLength(100).WithMessage("TypeName max length is 100.");

			RuleFor(x => x.Description)
				.MaximumLength(500).WithMessage("Description max length is 500.");
		}
	}
}
