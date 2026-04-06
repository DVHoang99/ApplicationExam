using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppExam.Application.Customers.Command;
using WebAppExam.Application.Customers.Commands;
using WebAppExam.Application.Customers.DTOs;
using WebAppExam.Application.Customers.Queries;

namespace WebAppExam.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CustomersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CustomerDTO input)
        {
            var command = CreateCustomerCommand.Create(input.CustomerName, input.Email, input.Phone);
            var id = await _mediator.Send(command);

            return Ok(new { data = id });
        }
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string phoneNumber = "",
            [FromQuery] string customerName = "",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10
            )
        {
            var query = GetAllCustomerQuery.GetAll(phoneNumber, customerName, pageNumber, pageSize);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Ulid id)
        {
            var query = GetCustomerByIdQuery.Init(id);
            var result = await _mediator.Send(query);
            return Ok(new { data = result });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Ulid id, [FromBody] CustomerDTO input)
        {
            var command = UpdateCustomerCommand.Init(id, input.CustomerName, input.Email, input.Phone);

            var result = await _mediator.Send(command);
            return Ok(new { data = result });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Ulid id)
        {
            var command = DeleteCustomerCommand.Init(id);
            await _mediator.Send(command);
            return NoContent();
        }
    }
}
