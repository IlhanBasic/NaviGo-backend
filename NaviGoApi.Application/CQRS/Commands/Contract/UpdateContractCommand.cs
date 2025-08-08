using MediatR;
using NaviGoApi.Application.DTOs.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.Contract
{
	public class UpdateContractCommand:IRequest<Unit>
	{
        public int Id { get; set; } 
        public ContractUpdateDto ContractDto {  get; set; }
        public UpdateContractCommand(int id, ContractUpdateDto dto)
        {
            ContractDto = dto;
            Id = id;
        }
    }
}
