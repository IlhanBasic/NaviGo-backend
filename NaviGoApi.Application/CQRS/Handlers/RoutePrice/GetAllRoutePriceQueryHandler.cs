using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Queries.RoutePrice;
using NaviGoApi.Application.DTOs.RoutePrice;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.RoutePrice
{
	public class GetAllRoutePriceQueryHandler : IRequestHandler<GetAllRoutePriceQuery, IEnumerable<RoutePriceDto?>>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;
		public GetAllRoutePriceQueryHandler(IMapper mapper, IUnitOfWork unitOfWork,IHttpContextAccessor httpContextAccessor)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
		}
		public async Task<IEnumerable<RoutePriceDto?>> Handle(GetAllRoutePriceQuery request, CancellationToken cancellationToken)
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

			var entities = (await _unitOfWork.RoutePrices.GetAllAsync()).ToList(); 
			var routes = (await _unitOfWork.Routes.GetAllAsync()).ToList();      
			var vehicleTypes = (await _unitOfWork.VehicleTypes.GetAllAsync()).ToList();

			var routesDict = routes.ToDictionary(r => r.Id);
			var vehicleTypesDict = vehicleTypes.ToDictionary(vt => vt.Id);

			Domain.Entities.Company? userCompany = null;
			if (user.CompanyId != null)
			{
				userCompany = await _unitOfWork.Companies.GetByIdAsync(user.CompanyId.Value)
					?? throw new ValidationException($"Company with ID {user.CompanyId.Value} doesn't exist.");
			}

			IEnumerable<Domain.Entities.RoutePrice> visibleEntities = entities;
			if (userCompany != null && userCompany.CompanyType == CompanyType.Carrier && user.UserRole != UserRole.SuperAdmin)
			{
				visibleEntities = entities.Where(e =>
				{
					if (!routesDict.TryGetValue(e.RouteId, out var route))
						return false; 
					return route.CompanyId == userCompany.Id;
				}).ToList();
			}

			var routePricesDto = visibleEntities.Select(e =>
			{
				vehicleTypesDict.TryGetValue(e.VehicleTypeId, out var vt);

				return new RoutePriceDto
				{
					Id = e.Id,
					RouteId = e.RouteId,
					VehicleTypeId = e.VehicleTypeId,
					PricePerKm = e.PricePerKm,
					PricePerKg = e.PricePerKg,
					MinimumPrice = e.MinimumPrice,
					VehicleTypeName = vt?.TypeName ?? string.Empty
				};
			}).ToList();

			return routePricesDto;
		}

	}

}
