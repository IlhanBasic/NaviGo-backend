using MediatR;
using NaviGoApi.Application.DTOs.ShipmentDocument;
using NaviGoApi.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Queries.ShipmentDocument
{
	public class GetAllShipmentDocumentQuery:IRequest<IEnumerable<ShipmentDocumentDto?>>
	{
        public ShipmentDocumentSearchDto Search {  get; set; }
        public GetAllShipmentDocumentQuery(ShipmentDocumentSearchDto search)
        {
            Search = search;
        }
    }
}
