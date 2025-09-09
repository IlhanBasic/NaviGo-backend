using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NaviGoApi.Application.CQRS.Commands.Vehicle;
using NaviGoApi.Application.CQRS.Queries.Vehicle;
using NaviGoApi.Application.DTOs.Vehicle;
using NaviGoApi.Common.DTOs;
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
		[Authorize]
		public async Task<ActionResult<IEnumerable<VehicleDto>>> GetAll([FromQuery] VehicleSearchDto searchDto)
		{
			var result = await _mediator.Send(new GetAllVehiclesQuery(searchDto));
			return Ok(result);
		}
		// GET: api/vehicle/available
		[HttpGet("available")]
		[Authorize]
		public async Task<ActionResult<IEnumerable<VehicleDto>>> GetAllAvailable()
		{
			var result = await _mediator.Send(new GetAllAvailableVehicleQuery());
			return Ok(result);
		}

		// GET: api/vehicle/{id}
		[HttpGet("{id}")]
		[Authorize]
		public async Task<ActionResult<VehicleDto>> GetById(int id)
		{
			var result = await _mediator.Send(new GetVehicleByIdQuery(id));
			if (result == null)
				return NotFound(new { message = $"Vehicle with ID {id} not found." });
			return Ok(result);
		}

		// POST: api/vehicle
		[HttpPost]
		[Authorize]
		public async Task<ActionResult<VehicleDto>> Create([FromBody] VehicleCreateDto createDto)
		{
			var createdVehicle = await _mediator.Send(new AddVehicleCommand(createDto));
			return CreatedAtAction(nameof(GetById), new { id = createdVehicle.Id }, createdVehicle);
		}

		// PUT: api/vehicle/{id}
		[HttpPut("{id}")]
		[Authorize]
		public async Task<ActionResult<VehicleDto>> Update(int id, [FromBody] VehicleUpdateDto updateDto)
		{
			var updatedVehicle = await _mediator.Send(new UpdateVehicleCommand(id, updateDto));
			if (updatedVehicle == null)
				return NotFound(new { message = $"Vehicle with ID {id} not found." });

			return Ok(updatedVehicle);
		}

		// DELETE: api/vehicle/{id}
		[HttpDelete("{id}")]
		[Authorize]
		public async Task<IActionResult> Delete(int id)
		{
			await _mediator.Send(new DeleteVehicleCommand(id));
			return NoContent();
		}
	}
}
