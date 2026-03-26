using MediatR;
using WebAppExam.Application.Orders.DTOs;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Orders.Queries;

public class GetOrderListQuery() : IRequest<List<OrderDto>>;

public class GetOrderListHandler : IRequestHandler<GetOrderListQuery, List<OrderDto>>
{
    private readonly IOrderRepository _repo;

    public GetOrderListHandler(IOrderRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<OrderDto>> Handle(GetOrderListQuery request, CancellationToken cancellationToken)
    {
        var orders = await _repo.GetAllAsync(cancellationToken);

        return orders.Select(order => new OrderDto
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            Status = order.Status.ToString(),
            Details = order.Details.Select(x => new OrderDetailDto
            {
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                Price = x.Price
            }).ToList()
        }).ToList();
    }
}