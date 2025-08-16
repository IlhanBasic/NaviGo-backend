using MediatR;
using NaviGoApi.Application.DTOs.User;
using NaviGoApi.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Queries.User
{
	public class GetAllUserQuery:IRequest<IEnumerable<UserDto>>
	{
        public UserSearchDto Search {  get; set; }
        public GetAllUserQuery(UserSearchDto search)
        {
            Search = search;
        }
    }
}
