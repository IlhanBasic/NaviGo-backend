using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Queries.Contract;
using NaviGoApi.Application.DTOs.Contract;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Contract
{
	public class GetAllContractQueryHandler : IRequestHandler<GetAllContractQuery, IEnumerable<ContractDto?>>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public GetAllContractQueryHandler(IMapper mapper, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<IEnumerable<ContractDto?>> Handle(GetAllContractQuery request, CancellationToken cancellationToken)
		{
			if (_httpContextAccessor.HttpContext == null)
				throw new ValidationException("HttpContext is null.");

			var userEmail = _httpContextAccessor.HttpContext.User
				.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

			if (string.IsNullOrEmpty(userEmail))
				throw new ValidationException("User email not found in token.");

			var user = await _unitOfWork.Users.GetByEmailAsync(userEmail)
				?? throw new ValidationException("User not found.");

			if (user.UserStatus != UserStatus.Active)
				throw new ValidationException("User must be activated.");

			var contracts = await _unitOfWork.Contracts.GetAllAsync(request.Search);

			var allUsers = await _unitOfWork.Users.GetAllAsync();
			var allCompanies = await _unitOfWork.Companies.GetAllAsync();
			var allRoutes = await _unitOfWork.Routes.GetAllAsync();

			if (user.UserRole == UserRole.CompanyAdmin && user.Company != null)
			{
				if (user.Company.CompanyType == CompanyType.Carrier)
				{
					contracts = contracts
						.Where(c => allRoutes.FirstOrDefault(r => r.Id == c.RouteId)?.CompanyId == user.CompanyId)
						.ToList();
				}
				else if (user.Company.CompanyType == CompanyType.Forwarder)
				{
					contracts = contracts.Where(c => c.ForwarderId == user.CompanyId).ToList();
				}
				else if (user.Company.CompanyType == CompanyType.Client)
				{
					contracts = contracts.Where(c => c.ClientId == user.Id).ToList();
				}
			}
			else if (user.UserRole == UserRole.RegularUser)
			{
				contracts = contracts.Where(c => c.ClientId == user.Id).ToList();
			}

			var contractDtos = contracts.Select(contract =>
			{
				var client = allUsers.FirstOrDefault(u => u.Id == contract.ClientId);
				var forwarder = allCompanies.FirstOrDefault(c => c.Id == contract.ForwarderId);

				return new ContractDto
				{
					Id = contract.Id,
					ClientId = contract.ClientId,
					ClientFullName = client != null ? $"{client.FirstName} {client.LastName}" : "",
					ForwarderId = contract.ForwarderId,
					ForwarderCompanyName = forwarder?.CompanyName ?? "",
					RouteId = contract.RouteId,
					ContractNumber = contract.ContractNumber,
					ContractDate = contract.ContractDate,
					Terms = contract.Terms,
					ContractStatus = contract.ContractStatus.ToString(),
					PenaltyRatePerHour = contract.PenaltyRatePerHour,
					MaxPenaltyPercent = contract.MaxPenaltyPercent,
					ValidUntil = contract.SignedDate?.AddDays(30) ?? DateTime.UtcNow,
					SignedDate = contract.SignedDate
				};
			}).ToList();

			return contractDtos;
		}

	}
}
