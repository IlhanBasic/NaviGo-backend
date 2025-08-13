using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.VehicleMaintenance;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.VehicleMaintenance
{
	public class AddVehicleMaintenanceCommandHandler : IRequestHandler<AddVehicleMaintenanceCommand, Unit>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;
		public AddVehicleMaintenanceCommandHandler(IMapper mapper, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        {
            _mapper = mapper;
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
        }
		public async Task<Unit> Handle(AddVehicleMaintenanceCommand request, CancellationToken cancellationToken)
		{
			if (_httpContextAccessor.HttpContext == null)
				throw new ValidationException("HttpContext is null.");
			var userEmail = _httpContextAccessor.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
			if (string.IsNullOrEmpty(userEmail))
				throw new ValidationException("User email not found in token.");
			var user = await _unitOfWork.Users.GetByEmailAsync(userEmail);
			if (user == null)
				throw new ValidationException("User not found.");
			var dto = request.Dto;
			var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(dto.VehicleId);
			if (vehicle == null)
			{
				throw new ValidationException($"Vehicle with ID {dto.VehicleId} does not exist.");
			}
			if (user.CompanyId != vehicle.CompanyId)
			{
				throw new ValidationException("User must belong to the same company as the vehicle.");
			}

			if (user.UserRole != UserRole.CompanyAdmin)
			{
				throw new ValidationException("Only users with CompanyAdmin role can report vehicle maintenance.");
			}
			var vehicleMaintenance = _mapper.Map<Domain.Entities.VehicleMaintenance>(dto);
			vehicleMaintenance.ReportedAt = DateTime.UtcNow;
			vehicleMaintenance.ReportedByUserId = user.Id;
			await _unitOfWork.VehicleMaintenances.AddAsync(vehicleMaintenance);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}

	}
}
