using MediatR;
using NaviGoApi.Application.DTOs.Shipment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.Shipment
{
	public class AddShipmentCommand:IRequest<Unit>
	{
        public ShipmentCreateDto ShipmentDto {  get; set; }
        public AddShipmentCommand(ShipmentCreateDto dto)
        {
			ShipmentDto=dto;

		}
    }
}
