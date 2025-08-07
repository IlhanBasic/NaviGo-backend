using MediatR;
using NaviGoApi.Application.DTOs.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.Driver
{
	public class AddDriverCommand:IRequest<Unit>
	{
        public DriverCreateDto DriverDto { get; set; }
        public AddDriverCommand(DriverCreateDto dto)
        {
            DriverDto = dto;
        }
    }
}
