using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Queries.Payment;
using NaviGoApi.Application.DTOs.Payment;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

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

		public async Task<IEnumerable<PaymentDto?>> Handle(GetAllPaymentQuery request, CancellationToken cancellationToken)
		{
			var httpContext = _httpContextAccessor.HttpContext
				?? throw new ValidationException("HttpContext is null.");

			var userEmail = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
			if (string.IsNullOrEmpty(userEmail))
				throw new ValidationException("User email not found in token.");

			var user = await _unitOfWork.Users.GetByEmailAsync(userEmail)
				?? throw new ValidationException("User not found.");

			if (user.UserStatus != UserStatus.Active)
				throw new ValidationException("User must be activated.");

			if (user.UserRole != UserRole.CompanyAdmin && user.UserRole != UserRole.RegularUser)
				throw new ValidationException("Only Company Admins or Regular Users can view payments.");

			// Učitaj sve potrebne podatke
			var allPayments = await _unitOfWork.Payments.GetAllAsync();
			var allContracts = await _unitOfWork.Contracts.GetAllAsync();
			var allRoutes = await _unitOfWork.Routes.GetAllAsync();
			var allUsers = await _unitOfWork.Users.GetAllAsync();

			IEnumerable<Domain.Entities.Payment> filteredPayments = allPayments;

			if (user.UserRole == UserRole.RegularUser)
			{
				// RegularUser vidi samo svoje paymente
				filteredPayments = allPayments.Where(p => p.ClientId == user.Id);
			}
			else if (user.UserRole == UserRole.CompanyAdmin)
			{
				if (user.Company == null)
					throw new ValidationException("Company not found for user.");

				if (user.Company.CompanyType == CompanyType.Carrier)
				{
					// Carrier -> vidi paymente vezane za svoje rute
					filteredPayments = allPayments.Where(p =>
					{
						var contract = allContracts.FirstOrDefault(c => c.Id == p.ContractId);
						if (contract == null) return false;

						var route = allRoutes.FirstOrDefault(r => r.Id == contract.RouteId);
						return route != null && route.CompanyId == user.Company.Id;
					});
				}
				else if (user.Company.CompanyType == CompanyType.Forwarder)
				{
					// Forwarder -> vidi paymente svojih ugovora
					filteredPayments = allPayments.Where(p =>
					{
						var contract = allContracts.FirstOrDefault(c => c.Id == p.ContractId);
						return contract != null && contract.ForwarderId == user.Company.Id;
					});
				}
				else if (user.Company.CompanyType == CompanyType.Client)
				{
					// CompanyAdmin za klijenta -> vidi paymente svojih zaposlenih
					filteredPayments = allPayments.Where(p =>
					{
						var client = allUsers.FirstOrDefault(u => u.Id == p.ClientId);
						return client != null && client.CompanyId == user.Company.Id;
					});
				}
				else
				{
					throw new ValidationException("This company type cannot view payments.");
				}
			}

			return filteredPayments.Select(p =>
			{
				var contract = allContracts.FirstOrDefault(c => c.Id == p.ContractId);
				var client = allUsers.FirstOrDefault(u => u.Id == p.ClientId);

				return new PaymentDto
				{
					Id = p.Id,
					ContractId = p.ContractId,
					Contract = contract?.ContractNumber ?? "",
					Amount = p.Amount,
					PaymentDate = p.PaymentDate,
					PaymentStatus = p.PaymentStatus.ToString(),
					ReceiptUrl = p.ReceiptUrl,
					ClientId = p.ClientId,
					Client = client != null ? $"{client.FirstName} {client.LastName}" : ""
				};
			});
		}
	}
}
