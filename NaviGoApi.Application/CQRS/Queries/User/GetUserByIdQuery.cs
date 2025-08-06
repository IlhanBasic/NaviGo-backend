using MediatR;
using NaviGoApi.Application.DTOs.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Queries.User
{
	public class GetUserByIdQuery : IRequest<UserDto?>
	{
		public int Id { get; }
		public GetUserByIdQuery(int id)
		{
			Id = id;
		}
	}
}
