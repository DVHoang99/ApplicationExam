using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebAppExam.Application.Customer.Queries;
using WebAppExam.Application.Order.Commands.CreateOrderCommand;

namespace WebAppExam.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly IMediator _mediator;
        public CustomerController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderCommand command)
        {
            var order = await _mediator.Send(command);
            return Ok(new { order });
        }

        [HttpGet]
        public async Task<IActionResult> Get(
                [FromQuery] string? phoneNumber,
                [FromQuery] string? customerName)
        {
            var result = await _mediator.Send(new GetCustomersQuery(phoneNumber, customerName)
            );

            return Ok(result);
        }
    }
}
