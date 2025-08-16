using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.Route;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Route
{
	public class DeleteRouteCommandHandler : IRequestHandler<DeleteRouteCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;
		public DeleteRouteCommandHandler(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
		{
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<Unit> Handle(DeleteRouteCommand request, CancellationToken cancellationToken)
		{
			var httpContext = _httpContextAccessor.HttpContext
				?? throw new InvalidOperationException("HttpContext is not available.");

			var userEmail = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
			if (string.IsNullOrWhiteSpace(userEmail))
				throw new ValidationException("User email not found in authentication token.");
			var user = await _unitOfWork.Users.GetByEmailAsync(userEmail)
				?? throw new ValidationException($"User with email '{userEmail}' not found.");
			if (user.CompanyId == null)
				throw new ValidationException("You don't work in any company.");
			var company = await _unitOfWork.Companies.GetByIdAsync(user.CompanyId.Value);
			if (company == null)
				throw new ValidationException($"Company with ID {user.CompanyId.Value} doesn't exists.");
			if (company.CompanyType != CompanyType.Carrier)
				throw new ValidationException("This company doesn't have right to delete routes. ");
			if (user.UserStatus != UserStatus.Active)
				throw new ValidationException("Your account is not activated.");
			if (user.UserRole == UserRole.RegularUser)
				throw new ValidationException("You are not allowed to delete company.");
			var route = await _unitOfWork.Routes.GetByIdAsync(request.Id);
			if (route == null)
				throw new ValidationException($"Route with ID {request.Id} not found");
			if (route.CompanyId != company.Id)
				throw new ValidationException("You cannot delete route for wrong company.");
			await _unitOfWork.Routes.DeleteAsync(route);
			await _unitOfWork.SaveChangesAsync();
			return Unit.Value;
		}
	}

}
