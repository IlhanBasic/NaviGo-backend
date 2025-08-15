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
	public class UpdateCompanyCommandHandler : IRequestHandler<UpdateCompanyCommand, CompanyDto>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IHttpContextAccessor _httpContextAccessor;
		public UpdateCompanyCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<CompanyDto> Handle(UpdateCompanyCommand request, CancellationToken cancellationToken)
		{
			var httpContext = _httpContextAccessor.HttpContext
				?? throw new InvalidOperationException("HttpContext is not available.");

			var userEmail = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
			if (string.IsNullOrWhiteSpace(userEmail))
				throw new ValidationException("User email not found in authentication token.");
			var user = await _unitOfWork.Users.GetByEmailAsync(userEmail)
				?? throw new ValidationException($"User with email '{userEmail}' not found.");
			if (user.UserStatus != UserStatus.Active)
				throw new ValidationException("User is not activated.");
			if (user.UserRole != UserRole.CompanyAdmin)
				throw new ValidationException("User must be CompanyAdmin for update company.");
			if (user.CompanyId == null)
				throw new ValidationException("User is RegularUser, so he don't have premission to change company.");
			var existing = await _unitOfWork.Companies.GetByIdAsync(request.Id)
	?? throw new ValidationException($"Company with ID {request.Id} not found.");
			if (user.CompanyId.Value != existing.Id)
				throw new ValidationException($"This user doesn't work in company with ID {existing.Id}");

			if (existing.CompanyType != CompanyType.Forwarder)
			{
				request.CompanyDto.MaxCommissionRate = null;
			}
			_mapper.Map(request.CompanyDto, existing);
			await _unitOfWork.Companies.UpdateAsync(existing);
			await _unitOfWork.SaveChangesAsync();
			return _mapper.Map<CompanyDto>(existing);
		}
	}
}
