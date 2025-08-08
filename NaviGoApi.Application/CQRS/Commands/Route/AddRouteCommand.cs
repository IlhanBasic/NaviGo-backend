using MediatR;
using NaviGoApi.Application.DTOs.Route;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.Route
{
	public class AddRouteCommand:IRequest<Unit>
	{
        public RouteCreateDto RouteDto { get; set; }
        public AddRouteCommand(RouteCreateDto dto)
        {
            RouteDto = dto;
        }
    }
}
