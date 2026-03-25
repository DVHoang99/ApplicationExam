using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebAppExam.Application.Order.Commands.CreateOrderCommand;

namespace WebAppExam.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IMediator _mediator;
        public OrderController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderCommand command)
        {
            var orderId = await _mediator.Send(command);
            return Ok(new { orderId });
        }

        //[HttpPost]
        //public async Task<IActionResult> AddProduct([FromBody] CreateOrderCommand command)
        //{
        //    var orderId = await _mediator.Send(command);
        //    return Ok(new { orderId });
        //}

        //[HttpGet("{id}")]
        //public async Task<IActionResult> AddProduct([FromQuery] string orderId)
        //{
        //    var order = await _mediator.Send(command);
        //    return Ok(new { orderId });
        //}
    }
}
