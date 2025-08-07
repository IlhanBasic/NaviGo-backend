using MediatR;
using Microsoft.AspNetCore.Mvc;
using NaviGoApi.Application.CQRS.Commands.Location;
using NaviGoApi.Application.CQRS.Queries.Location;
using NaviGoApi.Application.DTOs.Location;

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
		public async Task<IActionResult> Create([FromBody] LocationCreateDto dto)
		{
			var command = new AddLocationCommand(dto);
			await _mediator.Send(command);
			return Ok(new { message = "Location created successfully." });
		}

		// GET: api/location
		[HttpGet]
		public async Task<IActionResult> GetAll()
		{
			var query = new GetAllLocationQuery();
			var result = await _mediator.Send(query);
			return Ok(result);
		}

		// GET: api/location/{id}
		[HttpGet("{id}")]
		public async Task<IActionResult> GetById(int id)
		{
			var query = new GetLocationByIdQuery(id);
			var result = await _mediator.Send(query);

			if (result == null)
				return NotFound(new { message = $"Location with ID {id} not found." });

			return Ok(result);
		}

		// PUT: api/location/{id}
		[HttpPut("{id}")]
		public async Task<IActionResult> Update(int id, [FromBody] LocationUpdateDto dto)
		{
			if (id != dto.Id)
				return BadRequest(new { message = "ID in URL does not match ID in body." });

			var command = new UpdateLocationCommand(dto);
			await _mediator.Send(command);
			return Ok(new { message = "Location updated successfully." });
		}

		// DELETE: api/location/{id}
		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			var command = new DeleteLocationCommand(id);
			await _mediator.Send(command);
			return Ok(new { message = "Location deleted successfully." });
		}
	}
}
