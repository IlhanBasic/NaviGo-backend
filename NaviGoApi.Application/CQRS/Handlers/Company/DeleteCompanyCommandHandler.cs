using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.Company;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Company
{
	public class DeleteCompanyCommandHandler : IRequestHandler<DeleteCompanyCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;
		public DeleteCompanyCommandHandler(IUnitOfWork unitOfWork,IHttpContextAccessor httpContextAccessor)
		{
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<Unit> Handle(DeleteCompanyCommand request, CancellationToken cancellationToken)
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
				throw new ValidationException("You are not allowed to delete company.");
			var existing = await _unitOfWork.Companies.GetByIdAsync(request.Id);
			if (existing != null)
			{
				if (existing.Id != user.CompanyId)
					throw new ValidationException("You cannot delete wrong company.");
				await _unitOfWork.Companies.DeleteAsync(existing);
				await _unitOfWork.SaveChangesAsync();
			}

			return Unit.Value;
		}
	}
}
