using System;
using KafkaFlow.Producers;
using MediatR;
using WebAppExam.Application.Common.Caching;
using WebAppExam.Application.Orders.Events;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Orders.Commands;

public class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand, Ulid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cacheService;
    private readonly IProducerAccessor _producerAccessor;

    public UpdateOrderCommandHandler(IOrderRepository orderRepository,
    ICacheService cacheService,
    IProductRepository productRepository, IProducerAccessor producerAccessor)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _cacheService = cacheService;
        _producerAccessor = producerAccessor;
    }

    public async Task<Ulid> Handle(UpdateOrderCommand request, CancellationToken ct)
    {
        var order = await _orderRepository.GetByIdAsync(request.Id, ct);
        if (order == null)
        {
            var failure = new FluentValidation.Results.ValidationFailure("Order", "Order not found");
            throw new FluentValidation.ValidationException(new[] { failure });
        }

        var oldTotalAmount = order.TotalAmount;

        order.UpdateOrderGeneralInformation(request.CustomerId, request.CustomerName, request.Address, request.PhoneNumber);

        var products = await _productRepository.GetProductByIdsAsync(request.Items.Select(x => x.ProductId).ToList(), ct);

        foreach (var item in request.Items)
        {
            if (!products.ContainsKey(item.ProductId))
                continue;

            var product = products[item.ProductId];

            var inventory = product.Inventories.FirstOrDefault(x => x.Id == item.InventoryId);

            var inventoryId = inventory == null ? item.InventoryId : inventory.Id;


            order.AddOrUpdateItem(item.ProductId, product.Price, item.Quantity, inventoryId);
            product.AddOrUpdateInventory(inventoryId, -item.Quantity, product.Id, inventory.Name);
        }

        _orderRepository.Update(order);
        _productRepository.UpdateRange(products.Values.ToList());

        var producer = _producerAccessor.GetProducer("order-events-producer");

        await producer.ProduceAsync(
            order.Id.ToString(),
            new OrderCreatedIntegrationEvent(order.Id, order.TotalAmount - oldTotalAmount, DateTime.UtcNow, 0)
        );
        await _cacheService.RemoveByPrefixAsync($"order_detail:{request.Id}");
        return order.Id;
    }
}
