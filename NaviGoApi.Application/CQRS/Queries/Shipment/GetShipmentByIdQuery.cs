using MediatR;
using NaviGoApi.Application.DTOs.Shipment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Queries.Shipment
{
	public class GetShipmentByIdQuery:IRequest<ShipmentDto?>
	{
        public int Id { get; set; }
        public GetShipmentByIdQuery(int id)
        {
            Id=id;
        }
    }
}
