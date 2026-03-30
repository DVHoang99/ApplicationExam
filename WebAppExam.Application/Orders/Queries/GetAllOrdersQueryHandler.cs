using System;
using MediatR;
using WebAppExam.Application.Orders.DTOs;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Orders.Queries;

public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, List<OrderDto>>
{
    private readonly IOrderRepository _orderRepository;

    public GetAllOrdersQueryHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<List<OrderDto>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        var query = _orderRepository.Query();

        if(request.FromDate != null && request.ToDate != null)
        {
            query = _orderRepository.GetOrdersByDateQuery(query, request.FromDate, request.ToDate);
        }

        if (!string.IsNullOrWhiteSpace(request.CustomerName))
        {
            query = _orderRepository.GetCustomerByCustomerNameQuery(query, request.CustomerName);
        }

        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            query = _orderRepository.GetCustomerByPhoneNumberQuery(query, request.PhoneNumber);
        }


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
