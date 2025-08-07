using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.User
{
	public class ResetPasswordCommand : IRequest<bool>
	{
		public string Token { get; }
		public string NewPassword { get; }

		public ResetPasswordCommand(string token, string newPassword)
		{
			Token = token;
			NewPassword = newPassword;
		}
	}
}
