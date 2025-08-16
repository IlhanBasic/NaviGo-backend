using MediatR;
using NaviGoApi.Application.DTOs.Route;
using NaviGoApi.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Queries.Route
{
	public class GetAllRouteQuery:IRequest<IEnumerable<RouteDto?>>
	{
        public RouteSearchDto Search {  get; set; }
        public GetAllRouteQuery(RouteSearchDto search)
        {
            Search = search;
        }
    }
}
