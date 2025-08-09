using MediatR;
using NaviGoApi.Application.DTOs.ShipmentDocument;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.ShipmentDocument
{
	public class UpdateShipmentDocumentCommand:IRequest<Unit>
	{
        public int Id { get; set; }
        public ShipmentDocumentUpdateDto ShipmentDocumentDto { get; set; }
        public UpdateShipmentDocumentCommand(ShipmentDocumentUpdateDto dto)
        {
			ShipmentDocumentDto=dto;

		}
    }
}
