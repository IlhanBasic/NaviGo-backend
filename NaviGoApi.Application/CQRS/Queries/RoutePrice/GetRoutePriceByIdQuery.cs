using MediatR;
using NaviGoApi.Application.DTOs.RoutePrice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Queries.RoutePrice
{
	public class GetRoutePriceByIdQuery:IRequest<RoutePriceDto?>
	{
        public int Id { get; set; }
        public GetRoutePriceByIdQuery(int id)
        {
            Id=id;
        }
    }
}
