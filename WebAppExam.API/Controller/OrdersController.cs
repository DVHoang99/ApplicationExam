using MediatR;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize(Roles = "Admin")]
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
                InventoryId = x.InventoryId
            }).ToList()
        };
        var id = await _mediator.Send(command);
        return Ok(id);
    }
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string customerName = "")
    {
        var query = new GetAllOrdersQuery(customerName);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Ulid id)
    {
        var query = new GetOrderByIdQuery(id);
        var result = await _mediator.Send(query);
        return Ok(new { data = result });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
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
                Quantity = x.Quantity
            }).ToList()
        };

        var result = await _mediator.Send(command);
        return Ok(new { data = result });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Ulid id)
    {
        var command = new DeleteOrderCommand(id);
        await _mediator.Send(command);
        return NoContent();
    }
}