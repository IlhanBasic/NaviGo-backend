using AutoMapper;
using MediatR;
using NaviGoApi.Application.CQRS.Queries.User;
using NaviGoApi.Application.DTOs.User;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.User
{
	public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;

		public GetUserByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
		}

		public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
		{
			var user = await _unitOfWork.Users.GetByIdAsync(request.Id);
			return _mapper.Map<UserDto>(user);
		}
	}
}
