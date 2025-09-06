using MediatR;
using NaviGoApi.Application.DTOs.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.User
{
	public class UpdateUserCommand:IRequest<Unit>
	{
        public int Id { get; set; }
        public UserUpdateDto UserDto { get; set; }
        public UpdateUserCommand(int id, UserUpdateDto dto)
        {
            Id = id;            
            UserDto = dto;
        }
    }
}
