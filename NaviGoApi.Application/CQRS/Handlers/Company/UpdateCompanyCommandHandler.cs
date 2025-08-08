using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Commands.Company;
using NaviGoApi.Application.DTOs.Company;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Company
{
	public class UpdateCompanyCommandHandler : IRequestHandler<UpdateCompanyCommand, CompanyDto>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public UpdateCompanyCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		public async Task<CompanyDto> Handle(UpdateCompanyCommand request, CancellationToken cancellationToken)
		{
			var existing = await _unitOfWork.Companies.GetByIdAsync(request.Id);
			if (existing == null)
			{
				// Možeš baciti custom exception ili vratiti null, po dogovoru
				return null!;
			}

			_mapper.Map(request.CompanyDto, existing);
			_unitOfWork.Companies.Update(existing);
			await _unitOfWork.SaveChangesAsync();

			return _mapper.Map<CompanyDto>(existing);
		}
	}
}
