using MediatR;
using Microsoft.AspNetCore.Mvc;
using NaviGoApi.Application.CQRS.Commands.Contract;
using NaviGoApi.Application.CQRS.Queries.Contract;
using NaviGoApi.Application.DTOs.Contract;
using System.Collections.Generic; // Za KeyNotFoundException

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
		public async Task<IActionResult> GetAll()
		{
			var result = await _mediator.Send(new GetAllContractQuery());
			return Ok(result);
		}

		// GET: api/contract/{id}
		[HttpGet("{id:int}")]
		public async Task<IActionResult> GetById(int id)
		{
			var result = await _mediator.Send(new GetContractByIdQuery(id));
			if (result == null)
				return NotFound(new { error = "NotFound", message = $"Contract with ID {id} not found." });
			return Ok(result);
		}

		// POST: api/contract
		[HttpPost]
		public async Task<IActionResult> Create([FromBody] ContractCreateDto dto)
		{
			await _mediator.Send(new AddContractCommand(dto));
			return Ok(new { message = "Contract created successfully." });
		}

		// PUT: api/contract/{id}
		[HttpPut("{id:int}")]
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

		// DELETE: api/contract/{id}
		[HttpDelete("{id:int}")]
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
