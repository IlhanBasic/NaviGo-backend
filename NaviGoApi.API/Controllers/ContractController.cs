using MediatR;
using Microsoft.AspNetCore.Mvc;
using NaviGoApi.Application.CQRS.Commands.Contract;
using NaviGoApi.Application.CQRS.Queries.Contract;
using NaviGoApi.Application.DTOs.Contract;
using System.Threading.Tasks;

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
				return NotFound();
			return Ok(result);
		}

		// POST: api/contract
		[HttpPost]
		public async Task<IActionResult> Create([FromBody] ContractCreateDto dto)
		{
			await _mediator.Send(new AddContractCommand(dto));
			return Ok("Contract created successfully");
		}

		// PUT: api/contract/{id}
		[HttpPut("{id:int}")]
		public async Task<IActionResult> Update(int id, [FromBody] ContractUpdateDto dto)
		{

			await _mediator.Send(new UpdateContractCommand(id,dto));
			return Ok("Contract updated successfully");
		}

		// DELETE: api/contract/{id}
		[HttpDelete("{id:int}")]
		public async Task<IActionResult> Delete(int id)
		{
			await _mediator.Send(new DeleteContractCommand(id));
			return Ok("Contract deleted successfully");
		}
	}
}
