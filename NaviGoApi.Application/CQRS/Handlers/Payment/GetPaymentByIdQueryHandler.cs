using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Queries.Payment;
using NaviGoApi.Application.DTOs.Payment;
using NaviGoApi.Domain.Interfaces;
using NaviGoApi.Domain.Entities;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Payment
{
	public class GetPaymentByIdQueryHandler : IRequestHandler<GetPaymentByIdQuery, PaymentDto?>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public GetPaymentByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
		}

		//public async Task<PaymentDto?> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
		//{
		//	var httpContext = _httpContextAccessor.HttpContext
		//		?? throw new InvalidOperationException("HttpContext is not available.");

		//	var userEmail = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
		//	if (string.IsNullOrWhiteSpace(userEmail))
		//		throw new ValidationException("User email not found in authentication token.");

		//	var user = await _unitOfWork.Users.GetByEmailAsync(userEmail)
		//		?? throw new ValidationException($"User with email '{userEmail}' not found.");

		//	if (user.UserStatus != UserStatus.Active)
		//		throw new ValidationException("User must be active to view this payment.");

		//	var payment = await _unitOfWork.Payments.GetByIdAsync(request.Id);
		//	if (payment == null)
		//		return null;

		//	return _mapper.Map<PaymentDto>(payment);
		//}
		public async Task<PaymentDto?> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
		{
			var httpContext = _httpContextAccessor.HttpContext
				?? throw new InvalidOperationException("HttpContext is not available.");

			var userEmail = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
			if (string.IsNullOrWhiteSpace(userEmail))
				throw new ValidationException("User email not found in authentication token.");

			var user = await _unitOfWork.Users.GetByEmailAsync(userEmail)
				?? throw new ValidationException($"User with email '{userEmail}' not found.");

			if (user.UserStatus != UserStatus.Active)
				throw new ValidationException("User must be active to view this payment.");

			// Učitaj payment
			var payment = await _unitOfWork.Payments.GetByIdAsync(request.Id);
			if (payment == null)
				return null;

			// Ručno učitaj contract i client
			var contract = await _unitOfWork.Contracts.GetByIdAsync(payment.ContractId);
			var client = contract != null ? await _unitOfWork.Users.GetByIdAsync(contract.ClientId) : null;

			// Složi DTO
			var paymentDto = new PaymentDto
			{
				Id = payment.Id,
				ContractId = payment.ContractId,
				Contract = contract?.ContractNumber ?? "",
				Amount = payment.Amount,
				PaymentDate = payment.PaymentDate,
				PaymentStatus = payment.PaymentStatus.ToString(),
				ReceiptUrl = payment.ReceiptUrl,
				ClientId = payment.ClientId,
				Client = client != null ? $"{client.FirstName} {client.LastName}" : ""
			};

			return paymentDto;
		}

	}
}
