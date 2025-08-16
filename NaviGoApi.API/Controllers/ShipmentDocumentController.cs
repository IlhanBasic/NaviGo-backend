using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NaviGoApi.Application.CQRS.Commands.ShipmentDocument;
using NaviGoApi.Application.CQRS.Queries.ShipmentDocument;
using NaviGoApi.Application.DTOs.ShipmentDocument;
using NaviGoApi.Common.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaviGoApi.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ShipmentDocumentController : ControllerBase
	{
		private readonly IMediator _mediator;

		public ShipmentDocumentController(IMediator mediator)
		{
			_mediator = mediator;
		}

		// GET: api/ShipmentDocument
		[HttpGet]
		[Authorize]
		public async Task<ActionResult<IEnumerable<ShipmentDocumentDto>>> GetAll([FromQuery] ShipmentDocumentSearchDto search)
		{
			var documents = await _mediator.Send(new GetAllShipmentDocumentQuery(search));
			return Ok(documents);
		}

		// GET: api/ShipmentDocument/{id}
		[HttpGet("{id:int}")]
		[Authorize]
		public async Task<ActionResult<ShipmentDocumentDto>> GetById(int id)
		{
			var document = await _mediator.Send(new GetShipmentDocumentByIdQuery(id));
			if (document == null)
				return NotFound(new { message = $"Shipment document with Id {id} not found." });

			return Ok(document);
		}

		// POST: api/ShipmentDocument
		[HttpPost]
		[Authorize]
		public async Task<ActionResult> Create([FromBody] ShipmentDocumentCreateDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(new { message = "Invalid data.", errors = ModelState });

			await _mediator.Send(new AddShipmentDocumentCommand(dto));
			return CreatedAtAction(nameof(GetById), new { id = dto.ShipmentId }, new { message = "Shipment document created successfully." });
		}

		// PUT: api/ShipmentDocument/{id}
		[HttpPut("{id:int}")]
		[Authorize]
		public async Task<ActionResult> Update(int id, [FromBody] ShipmentDocumentUpdateDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(new { message = "Invalid data.", errors = ModelState });

			await _mediator.Send(new UpdateShipmentDocumentCommand(dto) { Id = id });
			return Ok(new { message = "Shipment document updated successfully." });
		}

		// DELETE: api/ShipmentDocument/{id}
		[HttpDelete("{id:int}")]
		[Authorize]
		public async Task<ActionResult> Delete(int id)
		{
			await _mediator.Send(new DeleteShipmentDocumentCommand(id));
			return Ok(new { message = "Shipment document deleted successfully." });
		}
	}
}
