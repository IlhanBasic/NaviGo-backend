using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NaviGoApi.Application.CQRS.Commands.Contract;
using NaviGoApi.Application.CQRS.Queries.Contract;
using NaviGoApi.Application.DTOs.Contract;
using NaviGoApi.Common.DTOs;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // Za KeyNotFoundException

namespace NaviGoApi.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ContractController : ControllerBase
	{
		private readonly IMediator _mediator;

		public ContractController(IMediator mediator)
		{
			_mediator = mediator;
		}

		// GET: api/contract
		[HttpGet]
		[Authorize]
		public async Task<IActionResult> GetAll([FromQuery] ContractSearchDto search)
		{
			var result = await _mediator.Send(new GetAllContractQuery(search));
			return Ok(result);
		}

		// GET: api/contract/{id}
		[HttpGet("{id:int}")]
		[Authorize]
		public async Task<IActionResult> GetById(int id)
		{
			var result = await _mediator.Send(new GetContractByIdQuery(id));
			if (result == null)
				return NotFound(new { error = "NotFound", message = $"Contract with ID {id} not found." });
			return Ok(result);
		}

		// POST: api/contract
		[HttpPost]
		[Authorize]
		public async Task<IActionResult> Create([FromBody] ContractCreateDto dto)
		{
			await _mediator.Send(new AddContractCommand(dto));
			return Ok(new { message = "Contract created successfully." });
		}

		// POST: api/contract/client
		[HttpPost("client")]
		[Authorize]
		public async Task<IActionResult> CreateClientContract([FromBody] ClientContractCreateDto dto)
		{
			if (dto == null)
				return BadRequest(new { message = "Contract data is required." });

			// Validacija DTO-a (FluentValidation će se automatski primeniti ako je registrovan)
			var command = new AddClientContractCommand(dto);
			await _mediator.Send(command);

			return Ok(new { message = "Contract created successfully." });
		}

		// PUT: api/contract/{id}
		[HttpPut("{id:int}")]
		[Authorize]
		public async Task<IActionResult> Update(int id, [FromBody] ContractUpdateDto dto)
		{
			try
			{
				await _mediator.Send(new UpdateContractCommand(id, dto));
				return Ok(new { message = "Contract updated successfully." });
			}
			catch (KeyNotFoundException)
			{
				return NotFound(new { error = "NotFound", message = $"Contract with ID {id} not found." });
			}
		}
		// PUT: api/contract/carrier/{id}/status
		[HttpPut("carrier/{id:int}/status")]
		[Authorize]
		public async Task<IActionResult> UpdateCarrierContractStatus(int id, [FromBody] CarrierContractStatusUpdateDto dto)
		{
			if (dto == null)
				return BadRequest(new { message = "Contract status data is required." });

			try
			{
				var command = new UpdateCarrierContractStatusCommand(id,dto);
				await _mediator.Send(command);
				return Ok(new { message = "Contract status updated successfully." });
			}
			catch (ValidationException ex)
			{
				return BadRequest(new { error = "ValidationError", message = ex.Message });
			}
			catch (KeyNotFoundException)
			{
				return NotFound(new { error = "NotFound", message = $"Contract with ID {id} not found." });
			}
		}

		// DELETE: api/contract/{id}
		[HttpDelete("{id:int}")]
		[Authorize]
		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				await _mediator.Send(new DeleteContractCommand(id));
				return Ok(new { message = "Contract deleted successfully." });
			}
			catch (KeyNotFoundException)
			{
				return NotFound(new { error = "NotFound", message = $"Contract with ID {id} not found." });
			}
		}
	}
}
