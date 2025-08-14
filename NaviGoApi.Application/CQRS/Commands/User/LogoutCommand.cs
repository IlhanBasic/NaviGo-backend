using MediatR;
using NaviGoApi.Application.DTOs.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.User
{
	public class LogoutCommand:IRequest<Unit>
	{
        public LogoutRequestDto LogoutRequestDto { get; set; }
        public LogoutCommand(LogoutRequestDto logoutRequestDto)
        {
            LogoutRequestDto = logoutRequestDto;
        }
    }
}
