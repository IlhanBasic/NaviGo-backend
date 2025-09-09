using MediatR;
using NaviGoApi.Application.DTOs.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.Contract
{
	public class UpdateCarrierContractStatusCommand:IRequest<Unit>
	{
        public CarrierContractStatusUpdateDto ContractDto { get; set; }
        public int Id { get; set; }
        public UpdateCarrierContractStatusCommand(int id, CarrierContractStatusUpdateDto dto)
        {
            Id=id;
            ContractDto = dto;
        }
    }
}
