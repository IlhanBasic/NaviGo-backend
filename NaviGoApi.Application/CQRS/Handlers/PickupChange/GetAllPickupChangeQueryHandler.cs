using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Queries.PickupChange;
using NaviGoApi.Application.DTOs.PickupChange;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.PickupChange
{
	public class GetAllPickupChangesQueryHandler : IRequestHandler<GetAllPickupChangesQuery, IEnumerable<PickupChangeDto>>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public GetAllPickupChangesQueryHandler(IMapper mapper, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<IEnumerable<PickupChangeDto>> Handle(GetAllPickupChangesQuery request, CancellationToken cancellationToken)
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

			if (!(user.UserRole == UserRole.RegularUser || user.UserRole == UserRole.CompanyAdmin))
				throw new ValidationException("You are not allowed to view pickup changes.");

			var allPickupChanges = (await _unitOfWork.PickupChanges.GetAllAsync()).ToList();
			var allShipments = (await _unitOfWork.Shipments.GetAllAsync()).ToList();
			var allContracts = (await _unitOfWork.Contracts.GetAllAsync()).ToList();
			var allRoutes = (await _unitOfWork.Routes.GetAllAsync()).ToList();

			IEnumerable<Domain.Entities.PickupChange> filteredChanges;

			if (user.UserRole == UserRole.RegularUser)
			{
				filteredChanges = allPickupChanges.Where(pc => pc.ClientId == user.Id);
			}
			else
			{
				filteredChanges = allPickupChanges.Where(pc =>
				{
					var shipment = allShipments.FirstOrDefault(s => s.Id == pc.ShipmentId);
					if (shipment == null) return false;

					var contract = allContracts.FirstOrDefault(c => c.Id == shipment.ContractId);
					if (contract == null) return false;

					var route = allRoutes.FirstOrDefault(r => r.Id == contract.RouteId);
					if (user.CompanyId != null && contract.ClientId != 0)
					{
						var client = allShipments.FirstOrDefault(s => s.Id == pc.ShipmentId)?.Contract?.ClientId;
						if (client != null && client == user.CompanyId) return true;
					}

					if (user.CompanyId != null && contract.ForwarderId == user.CompanyId) return true;

					if (route != null && route.CompanyId == user.CompanyId) return true;

					return false;
				}).ToList();
			}

			return filteredChanges.Select(pc => new PickupChangeDto
			{
				Id = pc.Id,
				ShipmentId = pc.ShipmentId,
				ClientId = pc.ClientId,
				OldTime = pc.OldTime,
				NewTime = pc.NewTime,
				ChangeCount = pc.ChangeCount,
				AdditionalFee = pc.AdditionalFee
			}).ToList();
		}

	}
}
