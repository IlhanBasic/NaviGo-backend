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
	public class GetAllRouteQueryHandler : IRequestHandler<GetAllRouteQuery, IEnumerable<RouteDto?>>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;
		public GetAllRouteQueryHandler(IMapper mapper, IUnitOfWork unitOfWork,IHttpContextAccessor httpContextAccessor)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<IEnumerable<RouteDto?>> Handle(GetAllRouteQuery request, CancellationToken cancellationToken)
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
			var routes = await _unitOfWork.Routes.GetAllAsync(request.Search);
			var routesDto = new List<RouteDto>();
			foreach(var r in routes)
			{
				var company = await _unitOfWork.Companies.GetByIdAsync(r.CompanyId);
				var start = await _unitOfWork.Locations.GetByIdAsync(r.StartLocationId);
				var end = await _unitOfWork.Locations.GetByIdAsync(r.EndLocationId);
				routesDto.Add(new RouteDto
				{
					Id = r.Id,
					AvailableFrom = r.AvailableFrom,
					AvailableTo = r.AvailableTo,
					CreatedAt = r.CreatedAt,
					IsActive = r.IsActive,
					CompanyId = r.CompanyId,
					DistanceKm = r.DistanceKm,
					EndLocationId = r.EndLocationId,
					EstimatedDurationHours = r.EstimatedDurationHours,
					GeometryEncoded = r.GeometryEncoded,
					StartLocationId = r.StartLocationId,
					StartLocationName = start != null ? $"{start.FullAddress}, {start.City}, {start.Country}":"",
					EndLocationName = end != null ? $"{end.FullAddress}, {end.City}, {end.Country}" : "",
					CompanyName = company!=null ? $"{company.CompanyName}":""
				});
			}
			//return _mapper.Map<IEnumerable<RouteDto?>>(routes);
			return routesDto;
		}
	}
}
