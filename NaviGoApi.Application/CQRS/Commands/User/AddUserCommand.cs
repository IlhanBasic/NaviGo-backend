using MediatR;
using NaviGoApi.Application.DTOs.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.User
{
	public class AddUserCommand : IRequest<UserDto>
	{
		public UserCreateDto UserDto { get; }
		public AddUserCommand(UserCreateDto userDto) => UserDto = userDto;
	}
}
