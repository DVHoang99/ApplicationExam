using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebAppExam.Application.Orders.Commands;
using WebAppExam.Application.Orders.DTOs;

namespace WebAppExam.API.Controller;

[ApiController]
[Route("api/orders")]
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
            Items = input.Details.Select(x => new OrderItemDto
            {
                ProductId = x.ProductId,
                Quantity = x.Quantity
            }).ToList()
        };
        var id = await _mediator.Send(command);
        return Ok(id);
    }
}