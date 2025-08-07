using MediatR;
using NaviGoApi.Application.DTOs.Company;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Queries.Company
{
	public class GetCompanyByIdQuery : IRequest<CompanyDto>
	{
		public int Id { get; set; }

		public GetCompanyByIdQuery(int id)
		{
			Id = id;
		}
	}
}
