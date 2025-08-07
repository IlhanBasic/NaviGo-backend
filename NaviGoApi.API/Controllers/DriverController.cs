using MediatR;
using Microsoft.AspNetCore.Mvc;
using NaviGoApi.Application.CQRS.Commands.Driver;
using NaviGoApi.Application.CQRS.Queries.Driver;
using NaviGoApi.Application.DTOs.Driver;

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
		public async Task<IActionResult> GetAll()
		{
			var result = await _mediator.Send(new GetAllDriverQuery());
			return Ok(result);
		}

		// GET: api/driver/5
		[HttpGet("{id}")]
		public async Task<IActionResult> GetById(int id)
		{
			var result = await _mediator.Send(new GetDriverByIdQuery(id));
			return result is not null ? Ok(result) : NotFound();
		}

		// POST: api/driver
		[HttpPost]
		public async Task<IActionResult> Create([FromBody] DriverCreateDto dto)
		{
			var result = await _mediator.Send(new AddDriverCommand(dto));
			return Ok(result);
		}

		// PUT: api/driver/5
		[HttpPut("{id}")]
		public async Task<IActionResult> Update(int id, [FromBody] DriverUpdateDto dto)
		{

			var result = await _mediator.Send(new UpdateDriverCommand(id,dto));
			return NoContent();
		}

		// DELETE: api/driver/5
		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			var result = await _mediator.Send(new DeleteDriverCommand(id));
			return NoContent();
		}
	}
}
