using MediatR;
using NaviGoApi.Application.DTOs.ShipmentDocument;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Queries.ShipmentDocument
{
	public class GetAllShipmentDocumentQuery:IRequest<IEnumerable<ShipmentDocumentDto?>>
	{
	}
}
