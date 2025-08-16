using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.PickupChange;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.PickupChange
{
	public class DeletePickupChangeCommandHandler : IRequestHandler<DeletePickupChangeCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public DeletePickupChangeCommandHandler(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
		{
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<Unit> Handle(DeletePickupChangeCommand request, CancellationToken cancellationToken)
		{
			var httpContext = _httpContextAccessor.HttpContext
				?? throw new ValidationException("HttpContext is null.");

			var userEmail = httpContext.User
				.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

			if (string.IsNullOrEmpty(userEmail))
				throw new ValidationException("User email not found in token.");

			var user = await _unitOfWork.Users.GetByEmailAsync(userEmail)
				?? throw new ValidationException("User not found.");
			if (user.UserStatus != UserStatus.Active)
				throw new ValidationException("User must be activated.");
			if (user.UserRole != UserRole.RegularUser)
				throw new ValidationException("You are not allowed to delete pickup change.");
			var pickupChange = await _unitOfWork.PickupChanges.GetByIdAsync(request.Id)
				?? throw new ValidationException($"PickupChange with Id {request.Id} not found.");
			await _unitOfWork.PickupChanges.DeleteAsync(request.Id);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}
	}
}
