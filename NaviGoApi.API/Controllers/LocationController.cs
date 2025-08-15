using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NaviGoApi.Application.CQRS.Commands.Location;
using NaviGoApi.Application.CQRS.Queries.Location;
using NaviGoApi.Application.DTOs.Location;
using System;
using System.Threading.Tasks;

namespace NaviGoApi.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class LocationController : ControllerBase
	{
		private readonly IMediator _mediator;

		public LocationController(IMediator mediator)
		{
			_mediator = mediator;
		}

		// POST: api/location
		[HttpPost]
		[Authorize]
		public async Task<IActionResult> Create([FromBody] LocationCreateDto dto)
		{
			var location = await _mediator.Send(new AddLocationCommand(dto));
			return Ok(location);
		}


		// GET: api/location
		[HttpGet]
		[Authorize]
		public async Task<IActionResult> GetAll()
		{
			var result = await _mediator.Send(new GetAllLocationQuery());
			return Ok(result);
		}

		// GET: api/location/{id}
		[HttpGet("{id}")]
		[Authorize]
		public async Task<IActionResult> GetById(int id)
		{
			var result = await _mediator.Send(new GetLocationByIdQuery(id));
			if (result == null)
				return NotFound(new { message = $"Location with ID {id} not found." });

			return Ok(result);
		}

		// PUT: api/location/{id}
		[HttpPut("{id}")]
		[Authorize]
		public async Task<IActionResult> Update(int id, [FromBody] LocationUpdateDto dto)
		{
			try
			{
				await _mediator.Send(new UpdateLocationCommand(id, dto));
				return Ok(new { message = "Location updated successfully." });
			}
			catch (KeyNotFoundException)
			{
				return NotFound(new { error = "NotFound", message = $"Location with ID {id} not found." });
			}
		}

		// DELETE: api/location/{id}
		[HttpDelete("{id}")]
		[Authorize]
		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				await _mediator.Send(new DeleteLocationCommand(id));
				return Ok(new { message = "Location deleted successfully." });
			}
			catch (KeyNotFoundException)
			{
				return NotFound(new { error = "NotFound", message = $"Location with ID {id} not found." });
			}
		}
	}
}
