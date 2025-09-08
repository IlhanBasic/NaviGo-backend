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
	public class GetRoutePriceByIdQueryHandler : IRequestHandler<GetRoutePriceByIdQuery, RoutePriceDto?>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;
		public GetRoutePriceByIdQueryHandler(IMapper mapper, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<RoutePriceDto?> Handle(GetRoutePriceByIdQuery request, CancellationToken cancellationToken)
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
			var entity = await _unitOfWork.RoutePrices.GetByIdAsync(request.Id);
			if (entity == null) return null;
			var vehicleType = await _unitOfWork.VehicleTypes.GetByIdAsync(entity.VehicleTypeId);
			var routePricedto = new RoutePriceDto
			{
				Id = entity.Id,
				MinimumPrice = entity.MinimumPrice,
				PricePerKm = entity.PricePerKm,
				PricePerKg = entity.PricePerKg,
				RouteId = entity.RouteId,
				VehicleTypeId = entity.VehicleTypeId,
				VehicleTypeName = vehicleType != null ? vehicleType.TypeName : ""
			};
			return routePricedto;
			//return _mapper.Map<RoutePriceDto>(entity);
		}
	}

}
