using Microsoft.AspNetCore.Mvc;
using MediatR;
using NaviGoApi.Application.DTOs.Route;
using NaviGoApi.Application.CQRS.Commands.Route;
using NaviGoApi.Application.CQRS.Queries.Route;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace NaviGoApi.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class RouteController : ControllerBase
	{
		private readonly IMediator _mediator;

		public RouteController(IMediator mediator)
		{
			_mediator = mediator;
		}

		// GET: api/Route
		[HttpGet]
		public async Task<ActionResult<IEnumerable<RouteDto>>> GetAll()
		{
			var routes = await _mediator.Send(new GetAllRouteQuery());
			return Ok(routes);
		}

		// GET: api/Route/{id}
		[HttpGet("{id}")]
		public async Task<ActionResult<RouteDto?>> GetById(int id)
		{
			var route = await _mediator.Send(new GetRouteByIdQuery(id));
			if (route == null)
				return NotFound();
			return Ok(route);
		}

		// POST: api/Route
		[HttpPost]
		public async Task<IActionResult> Create([FromBody] RouteCreateDto routeDto)
		{
			await _mediator.Send(new AddRouteCommand(routeDto));
			return StatusCode(201);
		}

		// PUT: api/Route/{id}
		[HttpPut("{id}")]
		public async Task<IActionResult> Update(int id, [FromBody] RouteUpdateDto routeDto)
		{
			await _mediator.Send(new UpdateRouteCommand(id, routeDto));
			return NoContent();
		}

		// DELETE: api/Route/{id}
		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			await _mediator.Send(new DeleteRouteCommand(id));
			return NoContent();
		}
	}
}
