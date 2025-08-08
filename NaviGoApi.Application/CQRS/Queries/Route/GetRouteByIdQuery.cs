using MediatR;
using NaviGoApi.Application.DTOs.Route;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Queries.Route
{
	public class GetRouteByIdQuery:IRequest<RouteDto?>
	{
        public int Id { get; set; }
        public GetRouteByIdQuery(int id)
        {
            Id=id;
        }
    }
}
