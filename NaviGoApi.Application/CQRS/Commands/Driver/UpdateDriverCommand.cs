using MediatR;
using NaviGoApi.Application.DTOs.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.Driver
{
	public class UpdateDriverCommand:IRequest<Unit>
	{
        public DriverUpdateDto DriverDto { get; set; }
        public int Id { get; set; }
        public UpdateDriverCommand(int id, DriverUpdateDto dto)
        {
            DriverDto = dto;    
            Id = id;
        }
    }
}
