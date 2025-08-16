using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Queries.VehicleMaintenance;
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
	public class GetAllVehicleMaintenanceQueryHandler : IRequestHandler<GetAllVehicleMaintenanceQuery, IEnumerable<Domain.Entities.VehicleMaintenance>>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;
        public GetAllVehicleMaintenanceQueryHandler(IMapper mapper,IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        {
            _mapper= mapper;
			_unitOfWork= unitOfWork;
			_httpContextAccessor= httpContextAccessor;
        }
        public async Task<IEnumerable<Domain.Entities.VehicleMaintenance>> Handle(GetAllVehicleMaintenanceQuery request, CancellationToken cancellationToken)
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
			var vehiclemaintenances = await _unitOfWork.VehicleMaintenances.GetAllAsync(request.Search);
			return _mapper.Map<IEnumerable<Domain.Entities.VehicleMaintenance>>(vehiclemaintenances);
		}
	}
}
