using MediatR;
using NaviGoApi.Application.DTOs.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Queries.Contract
{
	public class GetAllContractQuery:IRequest<IEnumerable<ContractDto?>>
	{
	}
}
