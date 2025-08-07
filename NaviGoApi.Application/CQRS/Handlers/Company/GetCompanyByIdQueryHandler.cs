using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Queries.Company;
using NaviGoApi.Application.DTOs.Company;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Company
{
	public class GetCompanyByIdQueryHandler : IRequestHandler<GetCompanyByIdQuery, CompanyDto>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public GetCompanyByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		public async Task<CompanyDto> Handle(GetCompanyByIdQuery request, CancellationToken cancellationToken)
		{
			var entity = await _unitOfWork.Companies.GetByIdAsync(request.Id);
			return _mapper.Map<CompanyDto>(entity);
		}
	}
}
