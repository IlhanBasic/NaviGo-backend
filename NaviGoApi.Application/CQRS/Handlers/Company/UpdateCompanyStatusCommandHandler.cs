using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.Company;
using NaviGoApi.Application.Services;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace NaviGoApi.Application.CQRS.Handlers.Company
{
	public class UpdateCompanyStatusCommandHandler : IRequestHandler<UpdateCompanyStatusCommand, Unit>
	{

		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly IEmailService _emailService;
		public UpdateCompanyStatusCommandHandler(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor, IEmailService emailService)
		{
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
			_emailService = emailService;
		}
		public async Task<Unit> Handle(UpdateCompanyStatusCommand request, CancellationToken cancellationToken)
		{
			var httpContext = _httpContextAccessor.HttpContext
				?? throw new InvalidOperationException("HttpContext is not available.");

			var userEmail = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
			if (string.IsNullOrWhiteSpace(userEmail))
				throw new ValidationException("User email not found in authentication token.");
			var user = await _unitOfWork.Users.GetByEmailAsync(userEmail)
				?? throw new ValidationException($"User with email '{userEmail}' not found.");
			if (user.UserStatus != UserStatus.Active)
				throw new ValidationException("User is not activated.");
			if (user.UserRole != UserRole.SuperAdmin)
				throw new ValidationException("You are not allowed to update company status.");
			var existing = await _unitOfWork.Companies.GetByIdAsync(request.Id)
	?? throw new ValidationException($"Company with ID {request.Id} not found.");
			existing.CompanyStatus = request.CompanyDto.CompanyStatus;
			await _unitOfWork.Companies.UpdateAsync(existing);
			await _unitOfWork.SaveChangesAsync();
			var allEmployees = await _unitOfWork.Users.GetAllAsync();
			var targetEmployees = allEmployees.Where(e => e.CompanyId == existing.Id).ToList();
			var emailTasks = targetEmployees.Select(emp => _emailService.SendEmailCompanyStatusNotification(emp.Email, existing));
			await Task.WhenAll(emailTasks);
			return Unit.Value;
		}
	}
}
