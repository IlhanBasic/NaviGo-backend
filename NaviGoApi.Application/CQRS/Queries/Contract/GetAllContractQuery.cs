using MediatR;
using NaviGoApi.Application.DTOs.Contract;
using NaviGoApi.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Queries.Contract
{
	public class GetAllContractQuery:IRequest<IEnumerable<ContractDto?>>
	{
        public ContractSearchDto Search { get; set; }
        public GetAllContractQuery(ContractSearchDto search)
        {
            Search = search;
        }
    }
}
