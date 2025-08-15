using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Driver
{
	public class DeleteDriverCommandHandler : IRequestHandler<DeleteDriverCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;
		public DeleteDriverCommandHandler(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
		{
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<Unit> Handle(DeleteDriverCommand request, CancellationToken cancellationToken)
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
				throw new ValidationException("You are not allowed to delete driver.");
			var driver = await _unitOfWork.Drivers.GetByIdAsync(request.Id);
			if (driver != null)
			{
				var company = await _unitOfWork.Companies.GetByIdAsync(driver.CompanyId);
				if (company == null)
					throw new ValidationException($"Company with ID {driver.CompanyId} doesn't exists.");
				if (company.Id != user.CompanyId)
					throw new ValidationException("You cannot delete driver to wrong company.");
				if (company.CompanyType != CompanyType.Carrier)
					throw new ValidationException("Company must be Carrier to have drivers.");
				await _unitOfWork.Drivers.DeleteAsync(driver);
				await _unitOfWork.SaveChangesAsync();
			}
			
			return Unit.Value;
		}
	}
}
