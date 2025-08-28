using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Queries.Payment;
using NaviGoApi.Application.DTOs.Payment;
using NaviGoApi.Domain.Interfaces;
using NaviGoApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Payment
{
	public class GetAllPaymentQueryHandler : IRequestHandler<GetAllPaymentQuery, IEnumerable<PaymentDto?>>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public GetAllPaymentQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_httpContextAccessor = httpContextAccessor;
		}

		//public async Task<IEnumerable<PaymentDto?>> Handle(GetAllPaymentQuery request, CancellationToken cancellationToken)
		//{
		//	var httpContext = _httpContextAccessor.HttpContext
		//		?? throw new InvalidOperationException("HttpContext is not available.");

		//	var userEmail = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
		//	if (string.IsNullOrWhiteSpace(userEmail))
		//		throw new ValidationException("User email not found in authentication token.");

		//	var user = await _unitOfWork.Users.GetByEmailAsync(userEmail)
		//		?? throw new ValidationException($"User with email '{userEmail}' not found.");

		//	if (user.UserStatus != UserStatus.Active)
		//		throw new ValidationException("User must be active to view payments.");

		//	var payments = await _unitOfWork.Payments.GetAllAsync();
		//	return _mapper.Map<IEnumerable<PaymentDto>>(payments);
		//}
		public async Task<IEnumerable<PaymentDto?>> Handle(GetAllPaymentQuery request, CancellationToken cancellationToken)
		{
			var httpContext = _httpContextAccessor.HttpContext
				?? throw new InvalidOperationException("HttpContext is not available.");

			var userEmail = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
			if (string.IsNullOrWhiteSpace(userEmail))
				throw new ValidationException("User email not found in authentication token.");

			var user = await _unitOfWork.Users.GetByEmailAsync(userEmail)
				?? throw new ValidationException($"User with email '{userEmail}' not found.");

			if (user.UserStatus != UserStatus.Active)
				throw new ValidationException("User must be active to view payments.");

			var payments = await _unitOfWork.Payments.GetAllAsync();

			var paymentDtos = new List<PaymentDto>();
			foreach (var payment in payments)
			{
				// Učitaj contract i client iz baze
				var contract = await _unitOfWork.Contracts.GetByIdAsync(payment.ContractId);
				var client = contract != null ? await _unitOfWork.Users.GetByIdAsync(contract.ClientId) : null;

				paymentDtos.Add(new PaymentDto
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
				});
			}

			return paymentDtos;
		}

	}
}
