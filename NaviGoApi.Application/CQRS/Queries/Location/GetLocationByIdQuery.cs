using MediatR;
using NaviGoApi.Application.DTOs.Location;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Queries.Location
{
	public class GetLocationByIdQuery:IRequest<LocationDto>
	{
        public int Id { get; set; }
        public GetLocationByIdQuery(int id)
        {
            Id = id;
        }
    }
}
