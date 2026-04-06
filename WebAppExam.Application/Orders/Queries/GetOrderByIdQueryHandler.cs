using MediatR;
using WebAppExam.Application.Common.Caching;
using WebAppExam.Application.Orders.DTOs;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Orders.Queries;

public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
    private readonly IOrderRepository _repo;
    private readonly ICacheService _cacheService;

    public GetOrderByIdQueryHandler(IOrderRepository repo, ICacheService cacheService)
    {
        _repo = repo;
        _cacheService = cacheService;
    }

    public async Task<OrderDto> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var orderDTO = await _cacheService.GetAsync($"order_detail:{request.Id}", async () =>
        {
            var order = await _repo.GetByIdAsync(request.Id, cancellationToken);

            if (order == null)
            {
                var failure = new FluentValidation.Results.ValidationFailure("Order", "Order not found.");
                throw new FluentValidation.ValidationException(new[] { failure });
            }

            var details = order.Details.Select(x => new OrderDetailDto
            {
                ProductId = x.ProductId,
                Quantity = x.Quantity,
                Price = x.Price,
            }).ToList();

            var res = new OrderDto
            {
                Id = order.Id,
                CustomerId = order.CustomerId,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                CustomerName = order.CustomerName,
                Address = order.Address,
                PhoneNumber = order.PhoneNumber,
                CreatedAt = order.CreatedAt,
                Details = details
            };
            return res;
        }, TimeSpan.FromDays(1), cancellationToken);

        return orderDTO == null ? new OrderDto() : orderDTO;
    }
}
