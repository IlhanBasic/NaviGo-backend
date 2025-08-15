using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.Location;
using NaviGoApi.Application.DTOs.Location;
using NaviGoApi.Application.Services;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Location
{
	public class AddLocationCommandHandler : IRequestHandler<AddLocationCommand, LocationDto?>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly ILocationService _locationService;
        public AddLocationCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor, ILocationService locationService)
        {
            _unitOfWork = unitOfWork;
			_mapper = mapper;
			_httpContextAccessor = httpContextAccessor;
			_locationService = locationService;
        }
		public async Task<LocationDto?> Handle(AddLocationCommand request, CancellationToken cancellationToken)
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
			if (user.UserRole == UserRole.RegularUser)
				throw new ValidationException("You are not allowed to add a location.");
			if (request.LocationDto.Latitude < -90 || request.LocationDto.Latitude > 90)
				throw new ValidationException("Latitude must be between -90 and 90.");
			if (request.LocationDto.Longitude < -180 || request.LocationDto.Longitude > 180)
				throw new ValidationException("Longitude must be between -180 and 180.");
			var location = await _locationService.GetOrCreateAsync(request.LocationDto);
			return location;

		}

	}
}
