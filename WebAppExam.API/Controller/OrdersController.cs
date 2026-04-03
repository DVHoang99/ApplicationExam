using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebAppExam.Application.Orders.Commands;
using WebAppExam.Application.Orders.DTOs;
using WebAppExam.Application.Orders.Queries;

namespace WebAppExam.API.Controller;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] OrderDto input)
    {
        var command = new CreateOrderCommand
        {
            CustomerId = input.CustomerId,
            Address = input.Address,
            PhoneNumber = input.PhoneNumber,
            CustomerName = input.CustomerName,
            Items = input.Details.Select(x => new OrderItemDto
            {
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                WareHouseId = x.WareHouseId
            }).ToList()
        };
        var id = await _mediator.Send(command);
        return Ok(new { data = id });
    }
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string customerName = "",
        [FromQuery] string phoneNumber = "",
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10
        )
    {
        var query = new GetAllOrdersQuery(fromDate, toDate, customerName, phoneNumber, pageNumber, pageSize);
        var result = await _mediator.Send(query);
        return Ok(new { data = result });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Ulid id)
    {
        var query = new GetOrderByIdQuery(id);
        var result = await _mediator.Send(query);
        return Ok(new { data = result });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Ulid id, [FromBody] OrderDto input)
    {
        var command = new UpdateOrderCommand(id)
        {
            CustomerId = input.CustomerId,
            CustomerName = input.CustomerName,
            Address = input.Address,
            PhoneNumber = input.PhoneNumber,
            Items = input.Details.Select(x => new OrderItemDto
            {
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                WareHouseId = x.WareHouseId
            }).ToList()
        };

        var result = await _mediator.Send(command);
        return Ok(new { data = result });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Ulid id)
    {
        var command = new DeleteOrderCommand(id);
        await _mediator.Send(command);
        return NoContent();
    }
}