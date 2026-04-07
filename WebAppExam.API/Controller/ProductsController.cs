using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAppExam.Application.Common.Errors;
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

            var result = await _mediator.Send(command);
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(ErrorResult.FromResult(result.Errors.Select(e => e.Message).ToList()));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string name = "",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = GetAllProductQuery.Init(name, pageNumber, pageSize);
            var result = await _mediator.Send(query);
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(ErrorResult.FromResult(result.Errors.Select(e => e.Message).ToList()));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Ulid id)
        {
            var query = GetProductByIdQuery.Init(id);
            var result = await _mediator.Send(query);
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(ErrorResult.FromResult(result.Errors.Select(e => e.Message).ToList()));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Ulid id, [FromBody] ProductRequestDTO input)
        {
            var command = UpdateProductCommand.Init(id, input);

            var result = await _mediator.Send(command);
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(ErrorResult.FromResult(result.Errors.Select(e => e.Message).ToList()));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Ulid id, [FromQuery] string wareHouseId)
        {
            var command = DeleteProductCommand.Init(id, wareHouseId);

            var result = await _mediator.Send(command);
            if (result.IsSuccess)
            {
                return Ok(result.Value);
            }

            return BadRequest(ErrorResult.FromResult(result.Errors.Select(e => e.Message).ToList()));
        }
    }
}
