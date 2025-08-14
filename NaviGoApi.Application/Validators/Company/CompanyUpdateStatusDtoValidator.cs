using FluentValidation;
using NaviGoApi.Application.DTOs.Company;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.Validators.Company
{
	public class CompanyUpdateStatusDtoValidator : AbstractValidator<CompanyUpdateStatusDto>
	{
        public CompanyUpdateStatusDtoValidator()
        {
			RuleFor(x => x.CompanyStatus)
				.IsInEnum().WithMessage("Invalid company status.");
		}
    }
}
