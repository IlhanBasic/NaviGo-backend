using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.User
{
	using AutoMapper;
	using global::NaviGoApi.Application.CQRS.Queries.User;
	using global::NaviGoApi.Application.DTOs.User;
	using KnjizaraApi.Domain.Interfaces;
	using MediatR;

	using System.Collections.Generic;
	using System.Threading;
	using System.Threading.Tasks;

	namespace NaviGoApi.Application.CQRS.Handlers.User
	{
		public class GetAllUserQueryHandler : IRequestHandler<GetAllUserQuery, IEnumerable<UserDto>>
		{
			private readonly IUserRepository _userRepository;
			private readonly IMapper _mapper;

			public GetAllUserQueryHandler(IUserRepository userRepository, IMapper mapper)
			{
				_userRepository = userRepository;
				_mapper = mapper;
			}

			public async Task<IEnumerable<UserDto>> Handle(GetAllUserQuery request, CancellationToken cancellationToken)
			{
				var users = await _userRepository.GetAllAsync();
				var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);
				return userDtos;
			}
		}
	}

}
