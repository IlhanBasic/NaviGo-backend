using MediatR;
using NaviGoApi.Application.DTOs.Company;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.Company
{
	public class UpdateCompanyStatusCommand:IRequest<Unit>
	{
        public CompanyUpdateStatusDto CompanyDto { get; set; }
        public int Id { get; set; }
        public UpdateCompanyStatusCommand(int id,CompanyUpdateStatusDto dto)
        {
            Id = id;
            CompanyDto = dto;
        }
    }
}
