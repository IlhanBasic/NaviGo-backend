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
		public int UserId { get; }
		public string CurrentPassword { get; }
		public string NewPassword { get; }

		public ChangePasswordCommand(int userId, string currentPassword, string newPassword)
		{
			UserId = userId;
			CurrentPassword = currentPassword;
			NewPassword = newPassword;
		}
	}
}
