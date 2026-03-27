using MediatR;
using Microsoft.AspNetCore.Http;
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
            var command = new CreateCustomerCommand
            {
                CustomerName = input.CustomerName,
                Email = input.Email,
                Phone = input.Phone
            };

            var id = await _mediator.Send(command);
            return Ok(id);
        }
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string phoneNumber = "",
            [FromQuery] string customerName = "")
        {
            var query = new GetAllCustomerQuery(phoneNumber, customerName);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Ulid id)
        {
            var query = new GetCustomerByIdQuery(id);
            var result = await _mediator.Send(query);
            return Ok(new { data = result });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Ulid id, [FromBody] CustomerDTO input)
        {
            var command = new UpdateCustomerCommand(id)
            {
                CustomerName = input.CustomerName,
                Email = input.Email,
                Phone = input.Phone
            };

            var result = await _mediator.Send(command);
            return Ok(new { data = result });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Ulid id)
        {
            var command = new DeleteCustomerCommand(id);
            await _mediator.Send(command);
            return NoContent();
        }
    }
}
