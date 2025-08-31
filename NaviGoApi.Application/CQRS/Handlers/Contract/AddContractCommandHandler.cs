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

			var client = await _unitOfWork.Users.GetByIdAsync(dto.ClientId)
				?? throw new ValidationException($"Client with ID {dto.ClientId} not found.");

			bool clientIsCompany = client.CompanyId != null;

			// --- Provera prava kreiranja ---
			if (clientIsCompany)
			{
				// Pravna lica: samo CompanyAdmin iz iste kompanije može kreirati
				var company = await _unitOfWork.Companies.GetByIdAsync(client.CompanyId.Value)
					?? throw new ValidationException($"Company with ID {client.CompanyId.Value} doesn't exist.");

				if (company.CompanyType != CompanyType.Client)
					throw new ValidationException("Company must be a Client type.");

				if (currentUser.UserRole != UserRole.CompanyAdmin || currentUser.CompanyId != client.CompanyId)
					throw new ValidationException("You are not authorized to create a contract for this company client.");
			}
			else
			{
				// Fizička lica: mora biti RegularUser
				if (currentUser.UserRole != UserRole.RegularUser)
					throw new ValidationException("Only RegularUsers (individual clients) can create contracts for themselves.");
			}

			var forwarder = await _unitOfWork.Companies.GetByIdAsync(dto.ForwarderId)
				?? throw new ValidationException($"Forwarder with ID {dto.ForwarderId} not found.");

			if (clientIsCompany && client.CompanyId == forwarder.Id)
				throw new ValidationException("Client and forwarder cannot be the same company.");

			var route = await _unitOfWork.Routes.GetByIdAsync(dto.RouteId)
				?? throw new ValidationException($"Route with ID {dto.RouteId} not found.");

			var forwarderRoutes = await _unitOfWork.ForwarderOffers.GetByForwarderIdAsync(forwarder.Id);
			if (!forwarderRoutes.Any(x => x.RouteId == dto.RouteId))
				throw new ValidationException("Forwarder does not have an offer for this selected route.");

			if (await _unitOfWork.Contracts.DuplicateContract(dto.ContractNumber))
				throw new ValidationException($"Contract with number {dto.ContractNumber} already exists.");

			if (dto.PenaltyRatePerHour < 0)
				throw new ValidationException("Penalty rate cannot be negative.");

			if (dto.MaxPenaltyPercent < 0 || dto.MaxPenaltyPercent > 100)
				throw new ValidationException("Max penalty percent must be between 0 and 100.");

			var routePrice = await _unitOfWork.RoutePrices.GetByIdAsync(dto.RoutePriceId);
			if (routePrice == null || routePrice.RouteId != route.Id)
				throw new ValidationException("Invalid RoutePrice for this route.");

			var forwarderOffer = await _unitOfWork.ForwarderOffers.GetByIdAsync(dto.ForwarderOfferId);
			if (forwarderOffer == null || forwarderOffer.RouteId != route.Id || forwarderOffer.ForwarderId != forwarder.Id)
				throw new ValidationException("Invalid ForwarderOffer for this route or forwarder.");

			var contractEntity = _mapper.Map<Domain.Entities.Contract>(dto);
			contractEntity.ContractStatus = ContractStatus.Pending;
			contractEntity.SignedDate = DateTime.UtcNow;
			contractEntity.ContractDate = DateTime.UtcNow;

			await _unitOfWork.Contracts.AddAsync(contractEntity);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}

	}
}
