using MediatR;
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

		[HttpGet]
		public async Task<ActionResult<IEnumerable<VehicleMaintenanceDto>>> GetAll()
		{
			var query = new GetAllVehicleMaintenanceQuery();
			var result = await _mediator.Send(query);
			return Ok(result);
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<VehicleMaintenanceDto>> GetById(int id)
		{
			var query = new GetVehicleMaintenanceByIdQuery(id);
			var result = await _mediator.Send(query);
			if (result == null)
				return NotFound();
			return Ok(result);
		}

		[HttpPost]
		public async Task<ActionResult<VehicleMaintenanceDto>> Create(VehicleMaintenanceCreateDto dto)
		{
			var command = new AddVehicleMaintenanceCommand(dto);
			var result = await _mediator.Send(command);
			return Ok(result);
		}

		[HttpPut("{id}")]
		public async Task<ActionResult> Update(int id, VehicleMaintenanceUpdateDto dto)
		{
			if (id != dto.Id)
				return BadRequest("ID mismatch");

			var command = new UpdateVehicleMaintenanceCommand(id, dto);
			await _mediator.Send(command);
			return NoContent();
		}

		[HttpDelete("{id}")]
		public async Task<ActionResult> Delete(int id)
		{
			var command = new DeleteVehicleMaintenanceCommand(id);
			await _mediator.Send(command);
			return NoContent();
		}
	}
}
