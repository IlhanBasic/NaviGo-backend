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
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.VehicleMaintenance
{
	public class GetVehicleMaintenanceByIdQueryHandler : IRequestHandler<GetVehicleMaintenanceByIdQuery, VehicleMaintenanceDto?>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;
		public GetVehicleMaintenanceByIdQueryHandler(IMapper mapper, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
		}
		public async Task<VehicleMaintenanceDto?> Handle(GetVehicleMaintenanceByIdQuery request, CancellationToken cancellationToken)
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
				throw new ValidationException("Only users with CompanyAdmin role can report vehicle maintenance.");
			var vehiclemaintenance = await _unitOfWork.VehicleMaintenances.GetByIdAsync(request.Id);
			if (vehiclemaintenance == null) return null;
			var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(vehiclemaintenance.VehicleId);
			var userReport = await _unitOfWork.Users.GetByIdAsync(vehiclemaintenance.ReportedByUserId);
			var vehicleMaintenancedto = new VehicleMaintenanceDto
			{
				Id = vehiclemaintenance.Id,
				ReportedAt = vehiclemaintenance.ReportedAt,
				ResolvedAt = vehiclemaintenance.ResolvedAt,
				Description = vehiclemaintenance.Description,
				MaintenanceType = vehiclemaintenance.MaintenanceType.ToString(),
				RepairCost = vehiclemaintenance.RepairCost,
				ReportedByUserId = vehiclemaintenance.ReportedByUserId,
				Severity = vehiclemaintenance.Severity.ToString(),
				VehicleId = vehiclemaintenance.VehicleId,
				VehicleName = vehicle != null ? $"{vehicle.Brand} - {vehicle.Model} ({vehicle.ManufactureYear})" : "",
				ReportedByUserEmail = userReport != null ? userReport.Email : ""
			};
			//return _mapper.Map<VehicleMaintenanceDto>(vehiclemaintenance);
			return vehicleMaintenancedto;
		}
	}
}
