using AutoMapper;
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
	public class UpdatePaymentCommandHandler : IRequestHandler<UpdatePaymentCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public UpdatePaymentCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<Unit> Handle(UpdatePaymentCommand request, CancellationToken cancellationToken)
		{
			var httpContext = _httpContextAccessor.HttpContext
				?? throw new InvalidOperationException("HttpContext is not available.");

			var userEmail = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
			if (string.IsNullOrWhiteSpace(userEmail))
				throw new ValidationException("User email not found in authentication token.");

			var user = await _unitOfWork.Users.GetByEmailAsync(userEmail)
				?? throw new ValidationException($"User with email '{userEmail}' not found.");

			if (user.UserStatus != UserStatus.Active)
				throw new ValidationException("User must be active to perform this operation.");

			if (user.CompanyId == null)
				throw new ValidationException("Only company users can update payments.");

			var company = await _unitOfWork.Companies.GetByIdAsync(user.CompanyId.Value)
				?? throw new ValidationException("Company not found.");

			if (company.CompanyStatus != CompanyStatus.Approved)
				throw new ValidationException("Company must be approved to perform this action.");

			if (company.CompanyType != CompanyType.Forwarder && company.CompanyType != CompanyType.Carrier)
				throw new ValidationException("Only Forwarder or Carrier companies can update payments.");

			if (user.UserRole != UserRole.CompanyUser && user.UserRole != UserRole.CompanyAdmin)
				throw new ValidationException("You must be a company user or company admin to update payments.");

			var payment = await _unitOfWork.Payments.GetByIdAsync(request.Id)
				?? throw new ValidationException($"Payment with Id {request.Id} not found.");

			payment.PaymentStatus = request.PaymentDto.PaymentStatus;

			await _unitOfWork.Payments.UpdateAsync(payment);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}
	}

}
