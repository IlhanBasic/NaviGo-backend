using MediatR;
using NaviGoApi.Application.DTOs.Company;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.Company
{
	public class AddCompanyCommand : IRequest<CompanyDto>
	{
		public CompanyCreateDto CompanyDto { get; set; }

		public AddCompanyCommand(CompanyCreateDto companyDto)
		{
			CompanyDto = companyDto;
		}
	}
}
