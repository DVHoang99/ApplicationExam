using System;
using MediatR;
using WebAppExam.Application.Orders.DTOs;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Orders.Queries;

public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, List<OrderDto>>
{
    private readonly IOrderRepository _repo;

    public GetAllOrdersQueryHandler(IOrderRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<OrderDto>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _repo.GetAllAsync(cancellationToken);

        return orders.Count == 0 ? new List<OrderDto>() : orders.Select(order => new OrderDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            Status = order.Status,
            TotalAmount = order.TotalAmount,
            CustomerName = order.CustomerName,
            Address = order.Address,
            PhoneNumber = order.PhoneNumber,
            Details = order.Details.Select(x => new OrderDetailDto
            {
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                Price = x.Price,
                InventoryId = x.InventoryId
            }).ToList()
        }).ToList();
    }
}
