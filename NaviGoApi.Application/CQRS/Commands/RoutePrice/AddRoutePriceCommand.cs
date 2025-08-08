using MediatR;
using NaviGoApi.Application.DTOs.RoutePrice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.RoutePrice
{
	public class AddRoutePriceCommand:IRequest<Unit>
	{
        public RoutePriceCreateDto RoutePriceDto { get; set; }
        public AddRoutePriceCommand(RoutePriceCreateDto dto)
        {
            RoutePriceDto = dto;
        }
    }
}
