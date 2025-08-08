using MediatR;
using NaviGoApi.Application.DTOs.RoutePrice;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.RoutePrice
{
	public class UpdateRoutePriceCommand:IRequest<Unit>
	{
        public RoutePriceUpdateDto RoutePriceDto { get; set; }
        public int Id { get; set; }
        public UpdateRoutePriceCommand(int id,RoutePriceUpdateDto dto)
        {
            RoutePriceDto = dto;
            Id = id;
        }
    }
}
