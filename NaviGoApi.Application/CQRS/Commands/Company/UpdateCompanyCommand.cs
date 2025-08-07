using MediatR;
using NaviGoApi.Application.DTOs.Company;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.Company
{
	public class UpdateCompanyCommand : IRequest<CompanyDto>
	{
		public CompanyUpdateDto CompanyDto { get; set; }

		public UpdateCompanyCommand(CompanyUpdateDto companyDto)
		{
			CompanyDto = companyDto;
		}
	}
}
