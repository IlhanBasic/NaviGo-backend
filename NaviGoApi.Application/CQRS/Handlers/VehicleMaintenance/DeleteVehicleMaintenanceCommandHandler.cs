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
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.VehicleMaintenance
{
	public class DeleteVehicleMaintenanceCommandHandler : IRequestHandler<DeleteVehicleMaintenanceCommand, Unit>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;
        public DeleteVehicleMaintenanceCommandHandler(IMapper mapper,IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        {
            _mapper= mapper;
			_unitOfWork= unitOfWork;
			_httpContextAccessor= httpContextAccessor;
        }
        public async Task<Unit> Handle(DeleteVehicleMaintenanceCommand request, CancellationToken cancellationToken)
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
			if(vehiclemaintenance != null)
			{
				var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(vehiclemaintenance.VehicleId);
				if (vehicle == null)
					throw new ValidationException($"Vehicle with ID {vehiclemaintenance.VehicleId} doesn't exists.");
				var company = await _unitOfWork.Companies.GetByIdAsync(vehicle.CompanyId);
				if (company == null)
					throw new ValidationException($"Company for Vehicle with ID {vehicle.Id} doesn't exists.");
				if (company.Id != user.CompanyId)
					throw new ValidationException("User must belong to the same company as the vehicle.");
				await _unitOfWork.VehicleMaintenances.DeleteAsync(vehiclemaintenance.Id);
				await _unitOfWork.SaveChangesAsync();
			}
			return Unit.Value;
		}
	}
}
