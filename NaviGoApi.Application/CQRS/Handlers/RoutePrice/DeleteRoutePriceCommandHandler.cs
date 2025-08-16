using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.RoutePrice;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.RoutePrice
{
	public class DeleteRoutePriceCommandHandler : IRequestHandler<DeleteRoutePriceCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;
		public DeleteRoutePriceCommandHandler(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
		{

			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<Unit> Handle(DeleteRoutePriceCommand request, CancellationToken cancellationToken)
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
			if (user.UserRole != UserRole.CompanyAdmin)
				throw new ValidationException("You are not allowed to add route price.");
			if (user.CompanyId == null)
				throw new ValidationException("User doesn't work in any company.");
			var routePrice = await _unitOfWork.RoutePrices.GetByIdAsync(request.Id);
			if (routePrice == null)
				throw new ValidationException($"Route price with ID {request.Id} doesn't exists.");
			var route = await _unitOfWork.Routes.GetByIdAsync(routePrice.RouteId);
			if (route == null)
				throw new ValidationException($"Route with ID {request.Id} doesn't exists.");
			var company = await _unitOfWork.Companies.GetByIdAsync(route.CompanyId);
			if (company == null)
				throw new ValidationException($"Company with ID {route.CompanyId} doesn't exists.");
			if (company.CompanyType != CompanyType.Carrier)
				throw new ValidationException("Company must be Carrier for removing route price.");
			if (route.CompanyId != user.CompanyId.Value)
				throw new ValidationException("You cannot delete route price for wrong company.");
			await _unitOfWork.RoutePrices.DeleteAsync(request.Id);
			await _unitOfWork.SaveChangesAsync();
			return Unit.Value;
		}
	}

}
