using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebAppExam.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly IMediator _mediator;
        public CustomerController(IMediator mediator) => _mediator = mediator;

        //[HttpPost]
        //public async Task<IActionResult> Create([FromBody] CreateOrderCommand command)
        //{
        //    var order = await _mediator.Send(command);
        //    return Ok(new { order });
        //}
    }
}
