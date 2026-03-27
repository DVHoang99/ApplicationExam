using System;
using MediatR;
using WebAppExam.Application.Orders.DTOs;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Orders.Queries;

public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
    private readonly IOrderRepository _repo;

    public GetOrderByIdHandler(IOrderRepository repo)
    {
        _repo = repo;
    }

    public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _repo.GetByIdAsync(request.Id, cancellationToken);

        if (order == null)
            throw new Exception("Order not found");

        var details = order.Details.Select(x => new OrderDetailDto
        {
            ProductId = x.ProductId,
            Quantity = x.Quantity,
            Price = x.Price
        }).ToList();

        return new OrderDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            CustomerName = order.CustomerName,
            Address = order.Address,
            PhoneNumber = order.PhoneNumber,
            Details = details
        };
    }
}
