using MediatR;
using NaviGoApi.Application.DTOs.ShipmentDocument;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.ShipmentDocument
{
	public class AddShipmentDocumentCommand:IRequest<Unit>
	{
        public ShipmentDocumentCreateDto ShipmentDocumentDto { get; set; }
        public AddShipmentDocumentCommand(ShipmentDocumentCreateDto dto)
        {
			ShipmentDocumentDto=dto;

		}
    }
}
