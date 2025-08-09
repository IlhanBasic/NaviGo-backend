using MediatR;
using NaviGoApi.Application.DTOs.Shipment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.Shipment
{
	public class UpdateShipmentCommand:IRequest<Unit>
	{
        public int Id { get; set; }
        public ShipmentUpdateDto ShipmentDto { get; set; }
        public UpdateShipmentCommand(int id,ShipmentUpdateDto dto)
        {
            ShipmentDto = dto;
            Id = id;
        }
    }
}
