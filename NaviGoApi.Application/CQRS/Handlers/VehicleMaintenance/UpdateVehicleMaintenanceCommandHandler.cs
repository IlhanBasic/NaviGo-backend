using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.VehicleMaintenance;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.VehicleMaintenance
{
	public class UpdateVehicleMaintenanceCommandHandler : IRequestHandler<UpdateVehicleMaintenanceCommand, Unit>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;
		public UpdateVehicleMaintenanceCommandHandler(IMapper mapper,IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        {
            _mapper = mapper;
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
        }
		public async Task<Unit> Handle(UpdateVehicleMaintenanceCommand request, CancellationToken cancellationToken)
		{
			if (_httpContextAccessor.HttpContext == null)
				throw new ValidationException("HttpContext is null.");
			var userEmail = _httpContextAccessor.HttpContext.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
			if (string.IsNullOrEmpty(userEmail))
				throw new ValidationException("User email not found in token.");
			var user = await _unitOfWork.Users.GetByEmailAsync(userEmail);
			if (user == null)
				throw new ValidationException("User not found.");
			var vehiclemaintenance = await _unitOfWork.VehicleMaintenances.GetByIdAsync(request.Id);
			if (vehiclemaintenance == null)
				throw new ValidationException($"VehicleMaintenance with ID {request.Id} not found.");
			_mapper.Map(request.Dto, vehiclemaintenance);
			await _unitOfWork.VehicleMaintenances.UpdateAsync(vehiclemaintenance);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}
	}
}
