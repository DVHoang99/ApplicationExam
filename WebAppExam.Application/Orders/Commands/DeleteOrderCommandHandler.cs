using System;
using KafkaFlow.Producers;
using MediatR;
using WebAppExam.Application.Common.Caching;
using WebAppExam.Application.Orders.Events;
using WebAppExam.Domain.Common;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Orders.Commands;

public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand, Ulid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cacheService;
    private readonly IProducerAccessor _producerAccessor;

    public DeleteOrderCommandHandler(IOrderRepository orderRepository, ICacheService cacheService, IProductRepository productRepository, IProducerAccessor producerAccessor)
    {
        _orderRepository = orderRepository;
        _cacheService = cacheService;
        _productRepository = productRepository;
        _producerAccessor = producerAccessor;
    }

    public async Task<Ulid> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.Id, cancellationToken);

        if (order == null)
        {
            var failure = new FluentValidation.Results.ValidationFailure("Order", "Order not found");
            throw new FluentValidation.ValidationException(new[] { failure });
        }

        order.DeleteOrder();
        var productIds = order.Details.Select(x => x.ProductId).ToList();

        var products = await _productRepository.GetProductByIdsAsync(productIds, cancellationToken);

        foreach (var item in order.Details)
        {
            var product = products[item.ProductId];
        }

        _orderRepository.Update(order);
        _productRepository.UpdateRange(products.Values.ToList());

        order.AddEventDomain(new OrderCreatedIntegrationEvent(order.Id, -order.TotalAmount, DateTime.UtcNow, -1));

        await _cacheService.RemoveByPrefixAsync($"order_detail:{request.Id}");
        return order.Id;
    }
}
