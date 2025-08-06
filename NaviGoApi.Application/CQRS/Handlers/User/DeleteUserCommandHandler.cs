using MediatR;
using NaviGoApi.Application.CQRS.Commands.User;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.User
{
	public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, bool>
	{
		private readonly IUnitOfWork _unitOfWork;

		public DeleteUserCommandHandler(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
		{
			var user = await _unitOfWork.Users.GetByIdAsync(request.Id);
			if (user == null)
				return false;

			_unitOfWork.Users.Remove(user);
			await _unitOfWork.SaveChangesAsync();
			return true;
		}
	}
}
