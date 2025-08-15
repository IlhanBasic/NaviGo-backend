using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.Location;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Location
{
	public class DeleteLocationCommandHandler : IRequestHandler<DeleteLocationCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;

        public DeleteLocationCommandHandler(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
			_httpContextAccessor= httpContextAccessor;
        }
        public async Task<Unit> Handle(DeleteLocationCommand request, CancellationToken cancellationToken)
		{
			var httpContext = _httpContextAccessor.HttpContext
				?? throw new InvalidOperationException("HttpContext is not available.");

			var userEmail = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
			if (string.IsNullOrWhiteSpace(userEmail))
				throw new ValidationException("User email not found in authentication token.");
			var user = await _unitOfWork.Users.GetByEmailAsync(userEmail)
				?? throw new ValidationException($"User with email '{userEmail}' not found.");
			if (user.UserStatus != UserStatus.Active)
				throw new ValidationException("Your account is not activated.");
			if (user.UserRole != UserRole.SuperAdmin)
				throw new ValidationException("You are not allowed to add vehicle.");
			var location = await _unitOfWork.Locations.GetByIdAsync(request.Id);
			if (location != null)
			{
				await _unitOfWork.Locations.DeleteAsync(location.Id);
				await _unitOfWork.SaveChangesAsync();
			}
			return Unit.Value;
		}
	}
}
