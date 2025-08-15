using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.Company;
using NaviGoApi.Application.DTOs.Company;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Company
{
	public class AddCompanyCommandHandler : IRequestHandler<AddCompanyCommand, CompanyDto>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IHttpContextAccessor _httpContextAccessor;
		public AddCompanyCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<CompanyDto> Handle(AddCompanyCommand request, CancellationToken cancellationToken)
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
				throw new ValidationException("You are not allowed to add company.");
			var dto = request.CompanyDto;
			if (dto.CompanyType != CompanyType.Forwarder && dto.MaxCommissionRate.HasValue)
			{
				throw new ValidationException("Only Forwarder companies can have MaxCommissionRate set.");
			}
			var exists = await _unitOfWork.Companies.GetByPibAsync(request.CompanyDto.PIB);
			if (exists != null)
				throw new ValidationException($"Company with PIB: ${request.CompanyDto.PIB} already exists.");
			if (request.CompanyDto.CompanyType != CompanyType.Forwarder)
			{
				request.CompanyDto.MaxCommissionRate = null;
			}
			var entity = _mapper.Map<Domain.Entities.Company>(request.CompanyDto);
			entity.CompanyStatus = CompanyStatus.Pending;
			await _unitOfWork.Companies.AddAsync(entity);
			await _unitOfWork.SaveChangesAsync();
			return _mapper.Map<CompanyDto>(entity);
		}
	}
}
