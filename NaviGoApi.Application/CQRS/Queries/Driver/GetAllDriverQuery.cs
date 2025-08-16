using MediatR;
using NaviGoApi.Application.DTOs.Driver;
using NaviGoApi.Common.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Queries.Driver
{
	public class GetAllDriverQuery:IRequest<IEnumerable<DriverDto?>>
	{
        public DriverSearchDto Search {  get; set; }
        public GetAllDriverQuery(DriverSearchDto search)
        {
            Search = search;
        }
    }
}
