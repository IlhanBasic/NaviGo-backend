using MediatR;
using NaviGoApi.Application.DTOs.Location;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.Location
{
	public class AddLocationCommand:IRequest<LocationDto?>
	{
        public LocationCreateDto LocationDto { get; set; }
        public AddLocationCommand(LocationCreateDto dto)
        {
            LocationDto = dto;
        }
    }
}
