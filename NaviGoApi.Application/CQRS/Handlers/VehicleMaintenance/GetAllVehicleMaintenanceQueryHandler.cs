using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Queries.VehicleMaintenance;
using NaviGoApi.Application.DTOs.VehicleMaintenance;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.VehicleMaintenance
{
	public class GetAllVehicleMaintenanceQueryHandler : IRequestHandler<GetAllVehicleMaintenanceQuery, IEnumerable<VehicleMaintenanceDto>>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public GetAllVehicleMaintenanceQueryHandler(IMapper mapper, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<IEnumerable<VehicleMaintenanceDto>> Handle(GetAllVehicleMaintenanceQuery request, CancellationToken cancellationToken)
		{
			var httpContext = _httpContextAccessor.HttpContext
				?? throw new InvalidOperationException("HttpContext is not available.");

			var userEmail = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
			if (string.IsNullOrWhiteSpace(userEmail))
				throw new ValidationException("User email not found in authentication token.");

			var user = await _unitOfWork.Users.GetByEmailAsync(userEmail)
				?? throw new ValidationException($"User with email '{userEmail}' not found.");

			if (user.UserStatus != UserStatus.Active)
				throw new ValidationException("Your account is not activated.");

			if (user.UserRole != UserRole.CompanyAdmin)
				throw new ValidationException("Only users with CompanyAdmin role can view vehicle maintenance.");

			if (!user.CompanyId.HasValue)
				throw new ValidationException("User is not assigned to a company.");

			// 1️⃣ Dohvati sva vozila korisnikove firme i napravi dictionary za O(1) lookup
			var companyVehicles = (await _unitOfWork.Vehicles.GetAllAsync())
								  .Where(v => v.CompanyId == user.CompanyId.Value)
								  .ToDictionary(v => v.Id, v => v);

			if (!companyVehicles.Any())
				return new List<VehicleMaintenanceDto>();

			// 2️⃣ Dohvati sve maintenance za ta vozila
			var allMaintenance = await _unitOfWork.VehicleMaintenances.GetAllAsync(request.Search);
			var maintenanceForCompany = allMaintenance
										.Where(vm => companyVehicles.ContainsKey(vm.VehicleId))
										.ToList();

			if (!maintenanceForCompany.Any())
				return new List<VehicleMaintenanceDto>();

			// 3️⃣ Dohvati sve korisnike koji su reportovali
			var userIds = maintenanceForCompany.Select(vm => vm.ReportedByUserId).Distinct().ToList();
			var users = (await _unitOfWork.Users.GetAllAsync())
						.Where(u => userIds.Contains(u.Id))
						.ToDictionary(u => u.Id, u => u);

			// 4️⃣ Mapiranje DTO koristeći pre-loaded dictionaries
			var vehicleMaintenancesDto = maintenanceForCompany.Select(vm =>
			{
				var vehicle = companyVehicles[vm.VehicleId];
				var reportedBy = users.ContainsKey(vm.ReportedByUserId) ? users[vm.ReportedByUserId] : null;

				return new VehicleMaintenanceDto
				{
					Id = vm.Id,
					ReportedAt = vm.ReportedAt,
					ResolvedAt = vm.ResolvedAt,
					Description = vm.Description,
					MaintenanceType = vm.MaintenanceType.ToString(),
					RepairCost = vm.RepairCost,
					ReportedByUserId = vm.ReportedByUserId,
					Severity = vm.Severity.ToString(),
					VehicleId = vm.VehicleId,
					VehicleName = $"{vehicle.Brand} - {vehicle.Model} ({vehicle.ManufactureYear})",
					ReportedByUserEmail = reportedBy != null ? reportedBy.Email : ""
				};
			}).ToList();

			return vehicleMaintenancesDto;
		}

	}
}
