using MediatR;
using NaviGoApi.Application.DTOs.ShipmentDocument;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Queries.ShipmentDocument
{
	public class GetShipmentDocumentByIdQuery:IRequest<ShipmentDocumentDto?>
	{
        public int Id { get; set; } 
        public GetShipmentDocumentByIdQuery(int id)
        {
            Id = id;
        }
    }
}
