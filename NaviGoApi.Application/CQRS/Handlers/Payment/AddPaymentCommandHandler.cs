using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.Payment;
using NaviGoApi.Application.Services;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Payment
{
	public class AddPaymentCommandHandler : IRequestHandler<AddPaymentCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IPaymentCalculatorService _paymentCalculator;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public AddPaymentCommandHandler(
			IUnitOfWork unitOfWork,
			IMapper mapper,
			IPaymentCalculatorService paymentCalculator,
			IHttpContextAccessor httpContextAccessor)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
			_paymentCalculator = paymentCalculator;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<Unit> Handle(AddPaymentCommand request, CancellationToken cancellationToken)
		{
			var httpContext = _httpContextAccessor.HttpContext
				?? throw new InvalidOperationException("HttpContext is not available.");

			var userEmail = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
			if (string.IsNullOrWhiteSpace(userEmail))
				throw new ValidationException("User email not found in authentication token.");

			var user = await _unitOfWork.Users.GetByEmailAsync(userEmail)
				?? throw new ValidationException($"User with email '{userEmail}' not found.");

			if (user.UserStatus != UserStatus.Active)
				throw new ValidationException("User must be activated.");
			var contract = await _unitOfWork.Contracts.GetByIdAsync(request.PaymentDto.ContractId)
				?? throw new ValidationException($"Contract with ID '{request.PaymentDto.ContractId}' not found.");

			if (contract.ContractStatus == ContractStatus.Cancelled || contract.ContractStatus == ContractStatus.Completed)
				throw new ValidationException("Contract is cancelled or completed and cannot be paid.");
			if (contract.ClientId != user.Id)
			{
				var contractClient = await _unitOfWork.Users.GetByIdAsync(contract.ClientId)
					?? throw new ValidationException($"Client with ID '{contract.ClientId}' not found.");

				if (user.CompanyId == null || contractClient.CompanyId == null || user.CompanyId != contractClient.CompanyId)
					throw new ValidationException("You are not authorized to pay for this contract.");
			}
			if (user.CompanyId != null)
			{
				var company = await _unitOfWork.Companies.GetByIdAsync(user.CompanyId.Value);
				if (company != null && company.CompanyType != CompanyType.Client)
					throw new ValidationException("User must work in Client company to make payments.");
			}
			var amount = await _paymentCalculator.CalculatePaymentAmountAsync(request.PaymentDto.ContractId);
			var entity = _mapper.Map<Domain.Entities.Payment>(request.PaymentDto);
			entity.PaymentStatus = PaymentStatus.Pending;
			entity.Amount = amount;
			entity.ClientId = user.Id;
			entity.PaymentDate = DateTime.UtcNow;

			await _unitOfWork.Payments.AddAsync(entity);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}
	}
}
