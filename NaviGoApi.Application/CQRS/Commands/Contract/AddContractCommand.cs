using MediatR;
using NaviGoApi.Application.DTOs.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.Contract
{
	public class AddContractCommand:IRequest<Unit>
	{
        public ContractCreateDto ContractDto { get; set; }
        public AddContractCommand(ContractCreateDto dto)
        {
            ContractDto = dto;
        }
    }
}
