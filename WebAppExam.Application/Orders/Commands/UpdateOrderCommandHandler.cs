using System;
using KafkaFlow.Producers;
using MediatR;
using WebAppExam.Application.Common.Caching;
using WebAppExam.Application.Orders.DTOs;
using WebAppExam.Application.Orders.Events;
using WebAppExam.Domain;
using WebAppExam.Domain.Common;
using WebAppExam.Domain.Enum;
using WebAppExam.Domain.Events;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Orders.Commands;

public class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand, Ulid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cacheService;
    private readonly ICacheLockService _lockService;

    public UpdateOrderCommandHandler(IOrderRepository orderRepository,
    ICacheService cacheService,
    IProductRepository productRepository,
    ICacheLockService lockService)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _cacheService = cacheService;
        _lockService = lockService;
    }

    public async Task<Ulid> Handle(UpdateOrderCommand request, CancellationToken ct)
    {
        var groupedItems = request.Items
        .GroupBy(x => new { x.ProductId, x.WareHouseId })
        .Select(g => new OrderItemDto
        {
            ProductId = g.Key.ProductId,
            WareHouseId = g.Key.WareHouseId,
            Quantity = g.Sum(x => x.Quantity)
        })
        .OrderBy(x => x.ProductId)
        .ToList();

        var lockKeys = groupedItems
            .Select(x => $"lock:inventory:{x.WareHouseId}:{x.ProductId}")
            .ToList();

        var lockToken = Guid.NewGuid().ToString();
        var acquiredLocks = new List<string>();
        try
        {
            acquiredLocks = await _lockService.AcquireMultipleLocksAsync(lockKeys, lockToken, TimeSpan.FromSeconds(10));

            if (!acquiredLocks.Any() && lockKeys.Any())
            {
                var failure = new FluentValidation.Results.ValidationFailure("System", "System is busy. Please try again later.");
                throw new FluentValidation.ValidationException(new[] { failure });
            }

            var order = await _orderRepository.GetByIdAsync(request.Id, ct);
            if (order == null)
            {
                var failure = new FluentValidation.Results.ValidationFailure("Order", "Order not found");
                throw new FluentValidation.ValidationException(new[] { failure });
            }

            var oldTotalAmount = order.TotalAmount;

            order.UpdateOrderGeneralInformation(request.CustomerId, request.CustomerName, request.Address, request.PhoneNumber);

            var products = await _productRepository.GetProductByIdsAsync(request.Items.Select(x => x.ProductId).ToList(), ct);
            var itemUpdated = new List<OrderDetail>();

            foreach (var item in request.Items)
            {
                if (!products.ContainsKey(item.ProductId))
                {
                    var failure = new FluentValidation.Results.ValidationFailure("Product", $"ProductId {item.ProductId} not found.");
                    throw new FluentValidation.ValidationException(new[] { failure });
                }

                var product = products[item.ProductId];

                itemUpdated.Add(order.AddOrUpdateItem(item.ProductId, product.Price, item.Quantity, Ulid.Parse(item.WareHouseId)));
            }

            order.UpdateOrderStatus(OrderStatus.Draft, "Updating...");

            _orderRepository.Update(order);
            _productRepository.UpdateRange(products.Values.ToList());

            var orderUpdateEvent = new OrderUpdatedEvent
            {
                OrderId = order.Id.ToString(),
                CustomerName = request.CustomerName,
                Items = itemUpdated.Select(x => new OrderItemEvent
                {
                    ProductId = x.ProductId.ToString(),
                    Quantity = x.Quantity,
                    WareHouseId = x.WareHouseId.ToString()
                }).ToList()
            };

            order.AddEventDomain(orderUpdateEvent);

            order.AddEventDomain(new OrderCreatedIntegrationEvent(order.Id, order.TotalAmount - oldTotalAmount, DateTime.UtcNow, 0));

            await _cacheService.RemoveByPrefixAsync($"order_detail:{request.Id}");
            return order.Id;
        }
        finally
        {
            if (acquiredLocks.Any())
            {
                await _lockService.ReleaseMultipleLocksAsync(acquiredLocks, lockToken);
            }
        }
    }
}
