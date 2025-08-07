using MediatR;
using NaviGoApi.Application.DTOs.Location;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.Location
{
	public class UpdateLocationCommand:IRequest<Unit>
	{
        public LocationUpdateDto LocationDto { get; set; }
        public UpdateLocationCommand(LocationUpdateDto dto)
        {
            LocationDto = dto;
        }
    }
}
