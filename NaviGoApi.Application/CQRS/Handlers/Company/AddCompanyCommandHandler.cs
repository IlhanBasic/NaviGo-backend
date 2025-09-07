using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.Company;
using NaviGoApi.Application.DTOs.Company;
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

namespace NaviGoApi.Application.CQRS.Handlers.Company
{
	public class AddCompanyCommandHandler : IRequestHandler<AddCompanyCommand, CompanyDto>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly IEmailService _emailService;
		public AddCompanyCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor,IEmailService emailService)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_httpContextAccessor = httpContextAccessor;
			_emailService = emailService;
		}

		public async Task<CompanyDto> Handle(AddCompanyCommand request, CancellationToken cancellationToken)
		{
			var exists = await _unitOfWork.Companies.GetByPibAsync(request.CompanyDto.PIB);
			if (exists != null)
				throw new ValidationException($"Company with PIB: {request.CompanyDto.PIB} already exists.");

			if (request.CompanyDto.CompanyType != CompanyType.Forwarder)
				request.CompanyDto.MaxCommissionRate = null;

			var entity = _mapper.Map<Domain.Entities.Company>(request.CompanyDto);
			entity.CompanyStatus = CompanyStatus.Pending;

			await _unitOfWork.Companies.AddAsync(entity);
			await _unitOfWork.SaveChangesAsync();
			var allUsers = await _unitOfWork.Users.GetAllAsync();
			var superAdmins = allUsers.Where(u => u.UserRole == UserRole.SuperAdmin).ToList();
			foreach (var admin in superAdmins)
			{
				await _emailService.SendEmailCheckCompanyNotification(admin.Email, entity);
			}

			return _mapper.Map<CompanyDto>(entity);
		}

	}
}
