using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        public async Task<IActionResult> Create([FromBody] ProductDTO product)
        {
            var command = new CreateProductCommand
            {
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Inventories = product.Inventories.Select(x => new InventoryDTO
                {
                    Stock = x.Stock,
                    Name = x.Name
                }).ToList()
            };

            var id = await _mediator.Send(command);
            return Ok(id);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string name = "")
        {
            var query = new GetAllProductQuery(name);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Ulid id)
        {
            var query = new GetProductByIdQuery(id);
            var result = await _mediator.Send(query);
            return Ok(new { data = result });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Ulid id, [FromBody] ProductDTO input)
        {
            var command = new UpdateProductCommand(id)
            {
                Name = input.Name,
                Description = input.Description,
                Price = input.Price,
                Inventories = input.Inventories.Select(x => new InventoryDTO
                {
                    Id = x.Id,
                    Stock = x.Stock,
                    Name = x.Name
                }).ToList()
            };

            var result = await _mediator.Send(command);
            return Ok(new { data = result });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Ulid id)
        {
            var command = new DeleteProductCommand(id);
            await _mediator.Send(command);
            return NoContent();
        }
    }
}
