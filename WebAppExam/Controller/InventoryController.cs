using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebAppExam.Application.Inventory.Command.CreateProductCommand;
using WebAppExam.Application.Inventory.Queries.GetInventoryQuery;
using WebAppExam.Application.Inventory.Queries.GetProductByIdQuery;

namespace WebAppExam.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly IMediator _mediator;
        public InventoryController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
        {
            var productId = await _mediator.Send(command);
            return Ok(new { productId });
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string productName = "", int pageNumber = 1, int pageSize = 20)
        {
            var query = new GetInventoryQuery(productName, pageNumber, pageSize);
            var result = await _mediator.Send(query);
            return Ok(new { data = result });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetail([FromQuery] Guid productId)
        {
            var query = new GetProductByIdQuery(productId);
            var result = await _mediator.Send(query);
            return Ok(new { data = result });
        }
    }
}
