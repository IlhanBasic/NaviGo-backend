using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Queries.Contract;
using NaviGoApi.Application.DTOs.Contract;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Contract
{
	public class GetAllContractQueryHandler:IRequestHandler<GetAllContractQuery,IEnumerable<ContractDto?>>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		public GetAllContractQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
		}

		public async Task<IEnumerable<ContractDto?>> Handle(GetAllContractQuery request, CancellationToken cancellationToken)
		{
			var contracts = await _unitOfWork.Contracts.GetAllAsync();
			return _mapper.Map<IEnumerable<ContractDto>>(contracts);
		}

	}
}
