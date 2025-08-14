using MediatR;
using NaviGoApi.Application.DTOs.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.User
{
	public class ActivateUserCommand:IRequest<Unit>
	{
        public UserActivationDto UserDto { get; set; }
        public int Id {  get; set; }
        public ActivateUserCommand(int id, UserActivationDto dto)
        {
            Id = id;
            UserDto = dto;
        }
    }
}
