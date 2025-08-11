using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.RoutePrice;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.RoutePrice
{
	//public class UpdateRoutePriceCommandHandler : IRequestHandler<UpdateRoutePriceCommand, Unit>
	//{
	//	private readonly IUnitOfWork _unitOfWork;
	//	private readonly IMapper _mapper;

	//	public UpdateRoutePriceCommandHandler(IMapper mapper, IUnitOfWork unitOfWork)
	//	{
	//		_mapper = mapper;
	//		_unitOfWork = unitOfWork;
	//	}

	//	public async Task<Unit> Handle(UpdateRoutePriceCommand request, CancellationToken cancellationToken)
	//	{
	//		var route = await _unitOfWork.Routes.GetByIdAsync(request.RoutePriceDto.RouteId);
	//		if (route == null)
	//			throw new ValidationException($"Route with ID {request.RoutePriceDto.RouteId} does not exist.");
	//		var routeCompanyId = route.CompanyId;
	//		if (routeCompanyId != request.RoutePriceDto.CompanyId)
	//			throw new ValidationException("Route does not have a valid associated company.");
	//		var entity = await _unitOfWork.RoutePrices.GetByIdAsync(request.Id);
	//		if (entity == null)
	//			throw new ValidationException($"RoutePrice with ID {request.Id} does not exist.");
	//		var vehicleTypeExists = await _unitOfWork.VehicleTypes.ExistsAsync(vt => vt.Id == request.RoutePriceDto.VehicleTypeId);
	//		if (!vehicleTypeExists)
	//			throw new ValidationException($"Vehicle type with ID {request.RoutePriceDto.VehicleTypeId} does not exist.");
	//		if (request.RoutePriceDto.PricePerKm < 0)
	//			throw new ValidationException("Price per km cannot be negative.");
	//		if (request.RoutePriceDto.MinimumPrice < 0)
	//			throw new ValidationException("Minimum price cannot be negative.");
	//		var exists = await _unitOfWork.RoutePrices.ExistsAsync(rp =>
	//			rp.Id != request.Id && 
	//			rp.RouteId == request.RoutePriceDto.RouteId &&
	//			rp.VehicleTypeId == request.RoutePriceDto.VehicleTypeId);
	//		if (exists)
	//			throw new ValidationException("Price for this route and vehicle type already exists.");
	//		_mapper.Map(request.RoutePriceDto, entity);
	//		await _unitOfWork.RoutePrices.UpdateAsync(entity);
	//		await _unitOfWork.SaveChangesAsync();
	//		return Unit.Value;
	//	}

	//}
	public class RoutePriceUpdateHandler : IRequestHandler<UpdateRoutePriceCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public RoutePriceUpdateHandler(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<Unit> Handle(UpdateRoutePriceCommand request, CancellationToken cancellationToken)
		{
			if (_httpContextAccessor.HttpContext == null)
				throw new Exception("HttpContext is null.");
			var claims = _httpContextAccessor.HttpContext.User.Claims;

			foreach (var claim in claims)
			{
				Console.WriteLine($"{claim.Type} = {claim.Value}");
			}

			var userEmail = _httpContextAccessor.HttpContext.User
				.FindFirst(ClaimTypes.Email)?.Value;

			if (string.IsNullOrEmpty(userEmail))
				throw new ValidationException("User email not found in token.");

			var user = await _unitOfWork.Users.GetByEmailAsync(userEmail);
			if (user == null)
				throw new ValidationException("User not found.");

			if (user.CompanyId == null)
				throw new ValidationException("User is not associated with any company.");

			var company = await _unitOfWork.Companies.GetByIdAsync(user.CompanyId.Value);
			if (company == null)
				throw new ValidationException("User's company not found.");

			if (company.CompanyType != Domain.Entities.CompanyType.Carrier)
				throw new ValidationException("Company is not a carrier.");

			var entity = await _unitOfWork.RoutePrices.GetByIdAsync(request.Id);
			if (entity == null)
				throw new ValidationException($"RoutePrice with ID {request.Id} does not exist.");

			var route = await _unitOfWork.Routes.GetByIdAsync(entity.RouteId);
			if (route == null)
				throw new ValidationException("Associated route not found.");

			if (route.CompanyId != user.CompanyId)
				throw new ValidationException("You cannot modify prices for routes not owned by your company.");

			// Dalje validacije i update
			if (request.RoutePriceDto.PricePerKm < 0)
				throw new ValidationException("Price per km cannot be negative.");

			if (request.RoutePriceDto.MinimumPrice < 0)
				throw new ValidationException("Minimum price cannot be negative.");

			var exists = await _unitOfWork.RoutePrices.ExistsAsync(rp =>
				rp.Id != request.Id &&
				rp.RouteId == request.RoutePriceDto.RouteId &&
				rp.VehicleTypeId == request.RoutePriceDto.VehicleTypeId);

			if (exists)
				throw new ValidationException("Price for this route and vehicle type already exists.");

			_mapper.Map(request.RoutePriceDto, entity);

			await _unitOfWork.RoutePrices.UpdateAsync(entity);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}
	}

}
