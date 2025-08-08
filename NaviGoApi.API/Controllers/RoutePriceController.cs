using MediatR;
using Microsoft.AspNetCore.Mvc;
using NaviGoApi.Application.CQRS.Commands.RoutePrice;
using NaviGoApi.Application.CQRS.Queries.RoutePrice;
using NaviGoApi.Application.DTOs.RoutePrice;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaviGoApi.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class RoutePriceController : ControllerBase
	{
		private readonly IMediator _mediator;

		public RoutePriceController(IMediator mediator)
		{
			_mediator = mediator;
		}

		// GET: api/RoutePrice
		[HttpGet]
		public async Task<ActionResult<IEnumerable<RoutePriceDto>>> GetAll()
		{
			var query = new GetAllRoutePriceQuery();
			var result = await _mediator.Send(query);
			return Ok(result);
		}

		// GET: api/RoutePrice/{id}
		[HttpGet("{id}")]
		public async Task<ActionResult<RoutePriceDto>> GetById(int id)
		{
			var query = new GetRoutePriceByIdQuery(id);
			var result = await _mediator.Send(query);
			if (result == null)
				return NotFound();
			return Ok(result);
		}

		// POST: api/RoutePrice
		[HttpPost]
		public async Task<IActionResult> Create([FromBody] RoutePriceCreateDto dto)
		{
			var command = new AddRoutePriceCommand(dto);
			await _mediator.Send(command);
			return Ok();
		}

		// PUT: api/RoutePrice/{id}
		[HttpPut("{id}")]
		public async Task<IActionResult> Update(int id, [FromBody] RoutePriceUpdateDto dto)
		{
			var command = new UpdateRoutePriceCommand(id, dto);
			await _mediator.Send(command);
			return NoContent();
		}

		// DELETE: api/RoutePrice/{id}
		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			var command = new DeleteRoutePriceCommand(id);
			await _mediator.Send(command);
			return NoContent();
		}
	}
}
