using MediatR;
using NaviGoApi.Application.DTOs.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.Contract
{
	public class AddClientContractCommand:IRequest<Unit>
	{
		public ClientContractCreateDto ContractDto { get; set; }
        public AddClientContractCommand(ClientContractCreateDto dto)
        {
            ContractDto = dto;
        }
    }
}
