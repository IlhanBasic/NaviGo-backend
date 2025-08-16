using MediatR;
using NaviGoApi.Application.DTOs.Shipment;
using NaviGoApi.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Queries.Shipment
{
	public class GetAllShipmentQuery:IRequest<IEnumerable<ShipmentDto?>>
	{
        public ShipmentSearchDto Search {  get; set; }
        public GetAllShipmentQuery(ShipmentSearchDto search)
        {
            Search= search;
        }
    }
}
