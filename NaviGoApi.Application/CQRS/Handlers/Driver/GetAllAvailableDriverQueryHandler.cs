using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Queries.Driver;
using NaviGoApi.Application.DTOs.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Driver
{
	public class GetAllAvailableDriverQueryHandler : IRequestHandler<GetAllAvailableDriverQuery, IEnumerable<DriverDto?>>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IHttpContextAccessor _httpContextAccessor;
		public GetAllAvailableDriverQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_httpContextAccessor = httpContextAccessor;
		}
		public async Task<IEnumerable<DriverDto?>> Handle(GetAllAvailableDriverQuery request, CancellationToken cancellationToken)
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
				throw new ValidationException("Only CompanyAdmin can view drivers.");

			if (!user.CompanyId.HasValue)
				throw new ValidationException("User is not assigned to a company.");

			var company = await _unitOfWork.Companies.GetByIdAsync(user.CompanyId.Value);
			if (company == null)
				throw new ValidationException("User's company does not exist.");
			if (company.CompanyType != CompanyType.Carrier)
				throw new ValidationException("Only Carrier companies can have drivers.");
			var companyDrivers = (await _unitOfWork.Drivers.GetAvailableDriversAsync(company.Id))
								 .ToList();
			if (!companyDrivers.Any())
				return new List<DriverDto>();

			var driversDto = companyDrivers.Select(d => new DriverDto
			{
				Id = d.Id,
				CompanyId = d.CompanyId,
				FirstName = d.FirstName,
				LastName = d.LastName,
				HireDate = d.HireDate,
				LicenseCategories = d.LicenseCategories,
				LicenseExpiry = d.LicenseExpiry,
				PhoneNumber = d.PhoneNumber,
				LicenseNumber = d.LicenseNumber,
				DriverStatus = d.DriverStatus.ToString(),
				CompanyName = company.CompanyName
			}).ToList();

			return driversDto;
		}
	}
}
