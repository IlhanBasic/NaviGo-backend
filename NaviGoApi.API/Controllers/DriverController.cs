using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NaviGoApi.Application.CQRS.Commands.Driver;
using NaviGoApi.Application.CQRS.Queries.Driver;
using NaviGoApi.Application.DTOs.Driver;
using System.Collections.Generic; // za KeyNotFoundException

namespace NaviGoApi.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class DriverController : ControllerBase
	{
		private readonly IMediator _mediator;

		public DriverController(IMediator mediator)
		{
			_mediator = mediator;
		}

		// GET: api/driver
		[HttpGet]
		[Authorize]
		public async Task<IActionResult> GetAll()
		{
			var result = await _mediator.Send(new GetAllDriverQuery());
			return Ok(result);
		}

		// GET: api/driver/5
		[HttpGet("{id}")]
		[Authorize]
		public async Task<IActionResult> GetById(int id)
		{
			var result = await _mediator.Send(new GetDriverByIdQuery(id));
			if (result == null)
				return NotFound(new { error = "NotFound", message = $"Driver with ID {id} not found." });

			return Ok(result);
		}

		// POST: api/driver
		[HttpPost]
		[Authorize]
		public async Task<IActionResult> Create([FromBody] DriverCreateDto dto)
		{
			await _mediator.Send(new AddDriverCommand(dto));
			return Ok(new { message = "Driver created successfully." });
		}


		// PUT: api/driver/5
		[HttpPut("{id}")]
		[Authorize]
		public async Task<IActionResult> Update(int id, [FromBody] DriverUpdateDto dto)
		{
			try
			{
				await _mediator.Send(new UpdateDriverCommand(id, dto));
				return Ok(new { message = "Driver updated successfully." });
			}
			catch (KeyNotFoundException)
			{
				return NotFound(new { error = "NotFound", message = $"Driver with ID {id} not found." });
			}
		}

		// DELETE: api/driver/5
		[HttpDelete("{id}")]
		[Authorize]
		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				await _mediator.Send(new DeleteDriverCommand(id));
				return Ok(new { message = "Driver deleted successfully." });
			}
			catch (KeyNotFoundException)
			{
				return NotFound(new { error = "NotFound", message = $"Driver with ID {id} not found." });
			}
		}
	}
}
