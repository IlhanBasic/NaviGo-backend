using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Queries.Vehicle;
using NaviGoApi.Application.DTOs.Vehicle;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Vehicle
{
	public class GetAllVehiclesQueryHandler : IRequestHandler<GetAllVehiclesQuery, IEnumerable<VehicleDto>>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IHttpContextAccessor _httpContextAccessor;
		public GetAllVehiclesQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<IEnumerable<VehicleDto>> Handle(GetAllVehiclesQuery request, CancellationToken cancellationToken)
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
			var vehicles = await _unitOfWork.Vehicles.GetAllAsync(request.Search);
			var vehiclesdto = new List<VehicleDto>();	
			foreach (var v in vehicles)
			{
				var company = await _unitOfWork.Companies.GetByIdAsync(v.CompanyId);
				var current = await _unitOfWork.Locations.GetByIdAsync(v.CurrentLocationId.Value);
				var type = await _unitOfWork.VehicleTypes.GetByIdAsync(v.VehicleTypeId);
				vehiclesdto.Add(new VehicleDto
				{
					Id = v.Id,
					CreatedAt = v.CreatedAt,
					Brand=v.Brand,
					ManufactureYear=v.ManufactureYear,
					Model = v.Model,
					CapacityKg=v.CapacityKg,
					Categories=v.Categories,
					CompanyId=v.CompanyId,
					CurrentLocationId=v.CurrentLocationId.Value,
					EngineCapacityCc=v.EngineCapacityCc,
					InsuranceExpiry=v.InsuranceExpiry,
					LastInspectionDate=v.LastInspectionDate,
					RegistrationNumber=v.RegistrationNumber,
					VehiclePicture=v.VehiclePicture,
					VehicleStatus=v.VehicleStatus.ToString(),
					VehicleTypeId = v.VehicleTypeId,
					CompanyName=company != null ? company.CompanyName : "",
					CurrentLocationName = current != null ? $"{current.FullAddress}, {current.City}, {current.Country}":"",
					VehicleTypeName = type != null ? type.TypeName : ""
				});
			}
			//return _mapper.Map<IEnumerable<VehicleDto>>(vehicles);
			return vehiclesdto;
		}

	}
}
