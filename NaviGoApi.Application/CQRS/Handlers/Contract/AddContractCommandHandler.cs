using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Commands.Contract;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
		//public async Task<Unit> Handle(AddContractCommand request, CancellationToken cancellationToken)
		//{
		//	var contractEntity = _mapper.Map<NaviGoApi.Domain.Entities.Contract>(request.ContractDto);
		//	contractEntity.ContractStatus = Domain.Entities.ContractStatus.Pending;
		//	await _unitOfWork.Contracts.AddAsync(contractEntity);
		//	await _unitOfWork.SaveChangesAsync();

		//	return Unit.Value;
		//}
		public async Task<Unit> Handle(AddContractCommand request, CancellationToken cancellationToken)
		{
			var dto = request.ContractDto;
			var client = await _unitOfWork.Users.GetByIdAsync(dto.ClientId);
			if (client == null)
				throw new ValidationException($"Client with ID {dto.ClientId} not found.");
			bool clientIsCompany = client.CompanyId != null;
			var forwarder = await _unitOfWork.Companies.GetByIdAsync(dto.ForwarderId);
			if (forwarder == null)
				throw new ValidationException($"Forwarder with ID {dto.ForwarderId} not found.");
			if (clientIsCompany && client.CompanyId == forwarder.Id)
				throw new ValidationException("Client and forwarder cannot be the same company.");
			var route = await _unitOfWork.Routes.GetByIdAsync(dto.RouteId);
			if (route == null)
				throw new ValidationException($"Route with ID {dto.RouteId} not found.");

			if (forwarder.Routes.Any(x=>x.Id==request.ContractDto.RouteId))
				throw new ValidationException("Forwarder does not have offer for this selected route.");
			var exists = await _unitOfWork.Contracts.ExistsAsync(c => c.ContractNumber == dto.ContractNumber);
			if (exists)
				throw new ValidationException($"Contract with number {dto.ContractNumber} already exists.");
			if (dto.ContractDate.Date > DateTime.UtcNow.Date)
				throw new ValidationException("Contract date cannot be in the future.");

			if (dto.ValidUntil <= dto.ContractDate)
				throw new ValidationException("ValidUntil must be after ContractDate.");

			if (dto.SignedDate.HasValue &&
				(dto.SignedDate.Value < dto.ContractDate || dto.SignedDate.Value > dto.ValidUntil))
				throw new ValidationException("Signed date must be between contract date and valid until.");
			if (dto.PenaltyRatePerHour < 0)
				throw new ValidationException("Penalty rate cannot be negative.");

			if (dto.MaxPenaltyPercent < 0 || dto.MaxPenaltyPercent > 100)
				throw new ValidationException("Max penalty percent must be between 0 and 100.");
			var contractEntity = _mapper.Map<NaviGoApi.Domain.Entities.Contract>(dto);
			contractEntity.ContractStatus = Domain.Entities.ContractStatus.Pending;

			await _unitOfWork.Contracts.AddAsync(contractEntity);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}


	}
}
