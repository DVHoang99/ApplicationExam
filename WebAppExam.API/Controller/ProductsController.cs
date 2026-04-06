using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppExam.Application.Products.Commands;
using WebAppExam.Application.Products.DTOs;
using WebAppExam.Application.Products.Queries;

namespace WebAppExam.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductRequestDTO product)
        {
            var command = CreateProductCommand.Init(product.Name, product.Description, product.Price, product.WareHouseId, product.Stock);

            var id = await _mediator.Send(command);
            return Ok(new { data = id });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string name = "",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = GetAllProductQuery.Init(name, pageNumber, pageSize);
            var result = await _mediator.Send(query);
            return Ok(new { data = result });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Ulid id)
        {
            var query = new GetProductByIdQuery(id);
            var result = await _mediator.Send(query);
            return Ok(new { data = result });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Ulid id, [FromBody] ProductRequestDTO input)
        {
            var command = new UpdateProductCommand(id)
            {
                Name = input.Name,
                Description = input.Description,
                Price = input.Price,
                WareHouseId = input.WareHouseId,
                Stock = input.Stock,
            };

            var result = await _mediator.Send(command);
            return Ok(new { data = result });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Ulid id, [FromQuery] string wareHouseId)
        {
            var command = new DeleteProductCommand(id, wareHouseId);
            await _mediator.Send(command);
            return NoContent();
        }
    }
}
