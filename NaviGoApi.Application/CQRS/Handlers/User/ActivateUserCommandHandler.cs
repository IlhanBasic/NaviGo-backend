using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.User;
using NaviGoApi.Application.Services;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.User
{
	public class ActivateUserCommandHandler:IRequestHandler<ActivateUserCommand,Unit>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly IEmailService _emailService;
        public ActivateUserCommandHandler(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor,IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
			_emailService = emailService;
        }

		public async Task<Unit> Handle(ActivateUserCommand request, CancellationToken cancellationToken)
		{
			var httpContext = _httpContextAccessor.HttpContext
				?? throw new InvalidOperationException("HttpContext is not available.");

			var userEmail = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
			if (string.IsNullOrWhiteSpace(userEmail))
				throw new ValidationException("User email not found in authentication token.");

			var currentUser = await _unitOfWork.Users.GetByEmailAsync(userEmail)
				?? throw new ValidationException($"User with email '{userEmail}' not found.");

			var targetUser = await _unitOfWork.Users.GetByIdAsync(request.Id);
			if (targetUser == null)
				throw new ValidationException($"User with ID {request.Id} doesn't exist.");

			if (currentUser.UserRole == Domain.Entities.UserRole.SuperAdmin)
			{
				if (request.UserDto.UserStatus == null)
					throw new ValidationException("UserStatus wasn't provided.");

				targetUser.UserStatus = request.UserDto.UserStatus.Value;
				await _emailService.SendEmailUserStatusNotification(targetUser.Email, targetUser);
			}
			else
			{
				if (currentUser.UserRole != Domain.Entities.UserRole.CompanyAdmin)
					throw new ValidationException("Only CompanyAdmin can change the UserRole.");
				var company = await _unitOfWork.Companies.GetByIdAsync(currentUser.CompanyId.Value);
				if (company == null)
					throw new ValidationException("Your company could not be found.");
				if (company.Id != targetUser.CompanyId)
					throw new ValidationException("You cannot activate a user for a different company.");
				targetUser.UserRole = Domain.Entities.UserRole.CompanyAdmin;
				await _emailService.SendEmailUserStatusNotification(targetUser.Email, targetUser);
			}

			await _unitOfWork.SaveChangesAsync();
			return Unit.Value;
		}

	}
}
