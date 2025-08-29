using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Queries.Route;
using NaviGoApi.Application.DTOs.Route;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Route
{
	public class GetRouteByIdQueryHandler : IRequestHandler<GetRouteByIdQuery, RouteDto?>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;
		public GetRouteByIdQueryHandler(IMapper mapper, IUnitOfWork unitOfWork,IHttpContextAccessor httpContextAccessor)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<RouteDto?> Handle(GetRouteByIdQuery request, CancellationToken cancellationToken)
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
			var route = await _unitOfWork.Routes.GetByIdAsync(request.Id);
			if (route == null)
				return null;
			var company = await _unitOfWork.Companies.GetByIdAsync(route.CompanyId);
			var start = await _unitOfWork.Locations.GetByIdAsync(route.StartLocationId);
			var end = await _unitOfWork.Locations.GetByIdAsync(route.EndLocationId);
			var routeDto = new RouteDto
			{
				Id = route.Id,
				AvailableFrom = route.AvailableFrom,
				AvailableTo = route.AvailableTo,
				CreatedAt = route.CreatedAt,
				IsActive = route.IsActive,
				CompanyId = route.CompanyId,
				DistanceKm = route.DistanceKm,
				EndLocationId = route.EndLocationId,
				EstimatedDurationHours = route.EstimatedDurationHours,
				GeometryEncoded = route.GeometryEncoded,
				StartLocationId = route.StartLocationId,
				StartLocationName = start != null ? $"{start.FullAddress}, {start.City}, {start.Country}" : "",
				EndLocationName = end != null ? $"{end.FullAddress}, {end.City}, {end.Country}" : "",
				CompanyName = company != null ? $"{company.CompanyName}" : ""
			};
			//return _mapper.Map<RouteDto>(route);
			return routeDto;
		}
	}
}
