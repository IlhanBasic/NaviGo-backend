using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.User
{
	public class ForgotPasswordCommand : IRequest<bool>
	{
		public string Email { get; }

		public ForgotPasswordCommand(string email)
		{
			Email = email;
		}
	}
}
