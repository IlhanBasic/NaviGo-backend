using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.Contract;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Contract
{
	public class AddContractCommandHandler : IRequestHandler<AddContractCommand, Unit>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public AddContractCommandHandler(IMapper mapper, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<Unit> Handle(AddContractCommand request, CancellationToken cancellationToken)
		{
			if (_httpContextAccessor.HttpContext == null)
				throw new ValidationException("HttpContext is null.");

			var userEmail = _httpContextAccessor.HttpContext.User
				.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

			if (string.IsNullOrEmpty(userEmail))
				throw new ValidationException("User email not found in token.");

			var currentUser = await _unitOfWork.Users.GetByEmailAsync(userEmail)
				?? throw new ValidationException("User not found.");

			var dto = request.ContractDto;

			// --- Provera prava kreiranja ugovora ---
			if (currentUser.UserRole != UserRole.CompanyAdmin)
				throw new ValidationException("Only CompanyAdmin users can create contracts.");

			if (currentUser.CompanyId == null)
				throw new ValidationException("User does not belong to any company.");

			var transportCompany = await _unitOfWork.Companies.GetByIdAsync(currentUser.CompanyId.Value)
				?? throw new ValidationException("Transport company not found.");
			if (transportCompany.CompanyType != CompanyType.Carrier)
				throw new ValidationException("Company must be Carrier.");
			// Provera rute: mora pripadati transportnoj kompaniji
			var route = await _unitOfWork.Routes.GetByIdAsync(dto.RouteId)
				?? throw new ValidationException($"Route with ID {dto.RouteId} not found.");

			if (route.CompanyId != transportCompany.Id)
				throw new ValidationException("You can only create contracts for your own routes.");

			// Automatsko dobijanje RoutePrice i ForwarderOffer
			var routePrice = await _unitOfWork.RoutePrices.GetByRouteIdAsync(route.Id);
			if (routePrice == null)
				throw new ValidationException("No RoutePrice defined for this route.");

			var forwarderOffer = await _unitOfWork.ForwarderOffers.GetByRouteIdAsync(route.Id);
			if (forwarderOffer == null)
				throw new ValidationException("No ForwarderOffer defined for this route.");

			// Klijent se bira ručno
			var client = await _unitOfWork.Users.GetByIdAsync(dto.ClientId)
				?? throw new ValidationException($"Client with ID {dto.ClientId} not found.");

			// Provera da klijent nije ista kompanija kao transportna
			if (client.CompanyId != null && client.CompanyId == transportCompany.Id)
				throw new ValidationException("Client and transport company cannot be the same company.");

			if (await _unitOfWork.Contracts.DuplicateContract(dto.ContractNumber))
				throw new ValidationException($"Contract with number {dto.ContractNumber} already exists.");

			if (dto.PenaltyRatePerHour < 0)
				throw new ValidationException("Penalty rate cannot be negative.");

			if (dto.MaxPenaltyPercent < 0 || dto.MaxPenaltyPercent > 100)
				throw new ValidationException("Max penalty percent must be between 0 and 100.");

			// --- Kreiranje ugovora ---
			var contractEntity = _mapper.Map<Domain.Entities.Contract>(dto);
			contractEntity.ContractStatus = ContractStatus.Pending;
			contractEntity.ContractDate = DateTime.UtcNow;
			contractEntity.SignedDate = DateTime.UtcNow;

			await _unitOfWork.Contracts.AddAsync(contractEntity);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}

	}
}
