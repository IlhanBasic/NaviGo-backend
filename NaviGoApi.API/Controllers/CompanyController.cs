using MediatR;
using Microsoft.AspNetCore.Mvc;
using NaviGoApi.Application.CQRS.Commands.Company;
using NaviGoApi.Application.CQRS.Queries.Company;
using NaviGoApi.Application.DTOs.Company;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaviGoApi.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CompanyController : ControllerBase
	{
		private readonly IMediator _mediator;

		public CompanyController(IMediator mediator)
		{
			_mediator = mediator;
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<CompanyDto>>> GetAll()
		{
			var companies = await _mediator.Send(new GetAllCompaniesQuery());
			return Ok(companies);
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<CompanyDto>> GetById(int id)
		{
			var company = await _mediator.Send(new GetCompanyByIdQuery(id));
			if (company == null) return NotFound();
			return Ok(company);
		}

		[HttpPost]
		public async Task<ActionResult<CompanyDto>> Create([FromBody] CompanyCreateDto dto)
		{
			var created = await _mediator.Send(new AddCompanyCommand(dto));
			return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
		}

		[HttpPut("{id}")]
		public async Task<ActionResult<CompanyDto>> Update(int id, [FromBody] CompanyUpdateDto dto)
		{
			if (id != dto.Id)
				return BadRequest("ID mismatch");

			var updated = await _mediator.Send(new UpdateCompanyCommand(dto));
			if (updated == null)
				return NotFound();

			return Ok(updated);
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			await _mediator.Send(new DeleteCompanyCommand(id));
			return NoContent();
		}
	}
}
