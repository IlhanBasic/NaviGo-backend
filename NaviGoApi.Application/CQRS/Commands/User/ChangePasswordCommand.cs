using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.User
{
	public class ChangePasswordCommand : IRequest<bool>
	{
		public string CurrentPassword { get; }
		public string NewPassword { get; }

		public ChangePasswordCommand(string currentPassword, string newPassword)
		{
			CurrentPassword = currentPassword;
			NewPassword = newPassword;
		}
	}
}
