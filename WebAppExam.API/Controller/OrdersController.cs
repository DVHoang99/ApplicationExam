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
    public async Task<IActionResult> Create([FromBody] OrderDTO input)
    {
        var command = CreateOrderCommand.Init(input);
        var result = await _mediator.Send(command);
        return Ok(result.Value);
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
        var query = GetAllOrdersQuery.Init(fromDate, toDate, customerName, phoneNumber, pageNumber, pageSize);
        var result = await _mediator.Send(query);
        return Ok(result.Value);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Ulid id)
    {
        var query = GetOrderByIdQuery.Init(id);
        var result = await _mediator.Send(query);
        return Ok(result.Value);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Ulid id, [FromBody] OrderDTO input)
    {
        var command = UpdateOrderCommand.Init(id, input);
        var result = await _mediator.Send(command);
        return Ok(result.Value);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Ulid id)
    {
        var command = DeleteOrderCommand.Init(id);
        var result = await _mediator.Send(command);
        return Ok(result.Value);
    }

    [HttpPost("{id}/canceled")]
    public async Task<IActionResult> Cancel(Ulid id)
    {
        var command = CancelOrderCommand.Init(id);
        var result = await _mediator.Send(command);
        return Ok(result.Value);
    }
}