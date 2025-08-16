using MediatR;
using NaviGoApi.Application.DTOs.Company;
using NaviGoApi.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Queries.Company
{
	public class GetAllCompaniesQuery : IRequest<IEnumerable<CompanyDto>>
	{
        public CompanySearchDto Search {  get; set; }
        public GetAllCompaniesQuery(CompanySearchDto search)
        {
            Search = search;
        }
    }
}
