using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.Payment;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Payment
{
	public class DeletePaymentCommandHandler : IRequestHandler<DeletePaymentCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public DeletePaymentCommandHandler(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
		{
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<Unit> Handle(DeletePaymentCommand request, CancellationToken cancellationToken)
		{
			var payment = await _unitOfWork.Payments.GetByIdAsync(request.Id);
			if (payment == null)
				throw new KeyNotFoundException($"Payment with Id {request.Id} not found.");

			var httpContext = _httpContextAccessor.HttpContext
				?? throw new InvalidOperationException("HttpContext is not available.");

			var userEmail = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
			if (string.IsNullOrWhiteSpace(userEmail))
				throw new ValidationException("User email not found in authentication token.");

			var user = await _unitOfWork.Users.GetByEmailAsync(userEmail)
				?? throw new ValidationException($"User with email '{userEmail}' not found.");

			if (user.UserStatus != UserStatus.Active)
				throw new ValidationException("User must be activated.");

			if (user.CompanyId == null)
			{
				if (user.UserRole != UserRole.RegularUser)
					throw new ValidationException("Only RegularUser without a company can delete payments.");

				if (payment.ClientId != user.Id)
					throw new ValidationException("You are not authorized to delete this payment.");
			}
			else
			{
				var company = await _unitOfWork.Companies.GetByIdAsync(user.CompanyId.Value)
					?? throw new ValidationException("User company doesn't exist.");

				if (company.CompanyStatus != CompanyStatus.Approved)
					throw new ValidationException("Company must be approved.");

				if (company.CompanyType != CompanyType.Client)
					throw new ValidationException("Only Client companies can delete payments.");

				if (user.UserRole != UserRole.CompanyUser && user.UserRole != UserRole.CompanyAdmin)
					throw new ValidationException("You must be a CompanyUser or CompanyAdmin to delete payments.");

				var contract = await _unitOfWork.Contracts.GetByIdAsync(payment.ContractId)
					?? throw new ValidationException($"Contract with Id '{payment.ContractId}' not found.");

				var contractClient = await _unitOfWork.Users.GetByIdAsync(contract.ClientId)
					?? throw new ValidationException($"Client with Id '{contract.ClientId}' not found.");

				if (contractClient.CompanyId != company.Id)
					throw new ValidationException("You are not authorized to delete this payment.");
			}
			await _unitOfWork.Payments.DeleteAsync(payment);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}
	}

}
