using MediatR;
using Microsoft.AspNetCore.Mvc;
using NaviGoApi.Application.CQRS.Commands.RoutePrice;
using NaviGoApi.Application.CQRS.Queries.RoutePrice;
using NaviGoApi.Application.DTOs.RoutePrice;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

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
			var result = await _mediator.Send(new GetAllRoutePriceQuery());
			return Ok(result);
		}

		// GET: api/RoutePrice/{id}
		[HttpGet("{id}")]
		public async Task<ActionResult<RoutePriceDto>> GetById(int id)
		{
			var result = await _mediator.Send(new GetRoutePriceByIdQuery(id));
			if (result == null)
				return NotFound(new { error = "NotFound", message = $"Route price with ID {id} not found." });

			return Ok(result);
		}

		// POST: api/RoutePrice
		[HttpPost]
		public async Task<IActionResult> Create([FromBody] RoutePriceCreateDto dto)
		{
			await _mediator.Send(new AddRoutePriceCommand(dto));
			return Ok(new { message = "Route price created successfully." });
		}

		// PUT: api/RoutePrice/{id}
		[HttpPut("{id}")]
		public async Task<IActionResult> Update(int id, [FromBody] RoutePriceUpdateDto dto)
		{
			try
			{
				await _mediator.Send(new UpdateRoutePriceCommand(id, dto));
				return Ok(new { message = "Route price updated successfully." });
			}
			catch (KeyNotFoundException)
			{
				return NotFound(new { error = "NotFound", message = $"Route price with ID {id} not found." });
			}
		}

		// DELETE: api/RoutePrice/{id}
		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				await _mediator.Send(new DeleteRoutePriceCommand(id));
				return Ok(new { message = "Route price deleted successfully." });
			}
			catch (KeyNotFoundException)
			{
				return NotFound(new { error = "NotFound", message = $"Route price with ID {id} not found." });
			}
		}
	}
}
