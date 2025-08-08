using MediatR;
using NaviGoApi.Application.DTOs.Route;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.Route
{
	public class UpdateRouteCommand:IRequest<Unit>
	{
        public int Id { get; set; }
        public RouteUpdateDto RouteDto {  get; set; }
        public UpdateRouteCommand(int id, RouteUpdateDto dto)
        {
            Id= id;
            RouteDto= dto;
        }
    }
}
