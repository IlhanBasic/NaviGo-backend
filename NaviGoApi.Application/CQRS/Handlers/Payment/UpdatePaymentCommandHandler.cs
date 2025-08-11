using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.Payment;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
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
			_mapper = mapper;
			_unitOfWork = unitOfWork;
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
			if (user.CompanyId == null || (user.CompanyId !=null && user.Company.CompanyType==Domain.Entities.CompanyType.Client))
				throw new ValidationException($"This user doesn't have premission to change this payment, only Forwarder or Carrier have that option.");
			var payment = await _unitOfWork.Payments.GetByIdAsync(request.Id);
			if (payment == null)
				throw new ValidationException($"Payment with Id {request.Id} not found.");
			
			payment.PaymentStatus = request.PaymentDto.PaymentStatus;
			await _unitOfWork.Payments.UpdateAsync(payment);
			await _unitOfWork.SaveChangesAsync();
			return Unit.Value;
		}

	}
}
