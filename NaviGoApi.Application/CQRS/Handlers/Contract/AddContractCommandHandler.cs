using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Commands.Contract;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Contract
{
	public class AddContractCommandHandler : IRequestHandler<AddContractCommand, Unit>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
        public AddContractCommandHandler(IMapper mapper, IUnitOfWork unitOfWork )
        {
            _mapper = mapper;
			_unitOfWork = unitOfWork;
        }
		public async Task<Unit> Handle(AddContractCommand request, CancellationToken cancellationToken)
		{
			var contractEntity = _mapper.Map<NaviGoApi.Domain.Entities.Contract>(request.ContractDto);
			contractEntity.ContractStatus = Domain.Entities.ContractStatus.Pending;
			await _unitOfWork.Contracts.AddAsync(contractEntity);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}

	}
}
