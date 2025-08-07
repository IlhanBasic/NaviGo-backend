using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.User
{
	public class VerifyEmailCommand : IRequest<bool>
	{
		public string Token { get; }

		public VerifyEmailCommand(string token)
		{
			Token = token;
		}
	}
}
