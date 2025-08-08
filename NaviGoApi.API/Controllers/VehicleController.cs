using MediatR;
using Microsoft.AspNetCore.Mvc;
using NaviGoApi.Application.CQRS.Commands.Vehicle;
using NaviGoApi.Application.CQRS.Queries.Vehicle;
using NaviGoApi.Application.DTOs.Vehicle;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaviGoApi.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class VehicleController : ControllerBase
	{
		private readonly IMediator _mediator;

		public VehicleController(IMediator mediator)
		{
			_mediator = mediator;
		}

		// GET: api/vehicle
		[HttpGet]
		public async Task<ActionResult<IEnumerable<VehicleDto>>> GetAll()
		{
			var query = new GetAllVehiclesQuery();
			var result = await _mediator.Send(query);
			return Ok(result);
		}

		// GET: api/vehicle/{id}
		[HttpGet("{id}")]
		public async Task<ActionResult<VehicleDto>> GetById(int id)
		{
			var query = new GetVehicleByIdQuery(id);
			var result = await _mediator.Send(query);
			if (result == null)
				return NotFound();
			return Ok(result);
		}

		// POST: api/vehicle
		[HttpPost]
		public async Task<ActionResult<VehicleDto>> Create([FromBody] VehicleCreateDto createDto)
		{
			var command = new AddVehicleCommand(createDto);
			var result = await _mediator.Send(command);
			return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
		}

		// PUT: api/vehicle/{id}
		[HttpPut("{id}")]
		public async Task<ActionResult<VehicleDto>> Update(int id, [FromBody] VehicleUpdateDto updateDto)
		{

			var command = new UpdateVehicleCommand(id,updateDto);
			var result = await _mediator.Send(command);
			return Ok(result);
		}

		// DELETE: api/vehicle/{id}
		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			var command = new DeleteVehicleCommand(id);
			await _mediator.Send(command);
			return NoContent();
		}
	}
}
