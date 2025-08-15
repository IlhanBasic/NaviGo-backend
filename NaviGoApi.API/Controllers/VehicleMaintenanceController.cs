using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NaviGoApi.Application.CQRS.Commands.VehicleMaintenance;
using NaviGoApi.Application.CQRS.Queries.VehicleMaintenance;
using NaviGoApi.Application.DTOs.VehicleMaintenance;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaviGoApi.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class VehicleMaintenanceController : ControllerBase
	{
		private readonly IMediator _mediator;

		public VehicleMaintenanceController(IMediator mediator)
		{
			_mediator = mediator;
		}

		// GET: api/vehiclemaintenance
		[HttpGet]
		[Authorize]
		public async Task<ActionResult<IEnumerable<VehicleMaintenanceDto>>> GetAll()
		{
			var result = await _mediator.Send(new GetAllVehicleMaintenanceQuery());
			return Ok(result);
		}

		// GET: api/vehiclemaintenance/{id}
		[HttpGet("{id}")]
		[Authorize]
		public async Task<ActionResult<VehicleMaintenanceDto>> GetById(int id)
		{
			var result = await _mediator.Send(new GetVehicleMaintenanceByIdQuery(id));
			if (result == null)
				return NotFound(new { message = $"VehicleMaintenance with ID {id} not found." });
			return Ok(result);
		}

		// POST: api/vehiclemaintenance
		[HttpPost]
		[Authorize]
		public async Task<IActionResult> Create([FromBody] VehicleMaintenanceCreateDto dto)
		{
			await _mediator.Send(new AddVehicleMaintenanceCommand(dto));
			return StatusCode(201); // Created, ali bez lokacije i bez podataka
		}

		// PUT: api/vehiclemaintenance/{id}
		[HttpPut("{id}")]
		[Authorize]
		public async Task<IActionResult> Update(int id, [FromBody] VehicleMaintenanceUpdateDto dto)
		{
			await _mediator.Send(new UpdateVehicleMaintenanceCommand(id, dto));
			return NoContent();
		}

		// DELETE: api/vehiclemaintenance/{id}
		[HttpDelete("{id}")]
		[Authorize]
		public async Task<IActionResult> Delete(int id)
		{
			await _mediator.Send(new DeleteVehicleMaintenanceCommand(id));
			return NoContent();
		}
	}
}
