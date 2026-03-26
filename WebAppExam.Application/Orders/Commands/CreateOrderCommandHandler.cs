using System;
using KafkaFlow.Producers;
using MediatR;
using WebAppExam.Application.Features.Orders.Events;
using WebAppExam.Application.Orders.Events;
using WebAppExam.Domain;
using WebAppExam.Domain.Repository;
using WebAppExam.Infrastructure.UnitOfWork;

namespace WebAppExam.Application.Orders.Commands;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Ulid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IProducerAccessor _producerAccessor;


    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IProducerAccessor producerAccessor)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _producerAccessor = producerAccessor;
    }

    public async Task<Ulid> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        var customerExists = await _orderRepository.GetByIdAsync(request.CustomerId, ct);

        if (customerExists == null)
            throw new KeyNotFoundException("Customer not found.");

        var order = new Order(request.CustomerId);

        var products = await _productRepository.GetProductByIdsAsync(request.Items.Select(x => x.ProductId).ToList(), ct);

        foreach (var item in request.Items)
        {
            if (!products.ContainsKey(item.ProductId))
                continue;

            var product = products[item.ProductId];

            order.AddOrUpdateItem(item.ProductId, product.Price, item.Quantity);
        }

        await _orderRepository.AddAsync(order, ct);

        var producer = _producerAccessor.GetProducer("order-events-producer");
        
        await producer.ProduceAsync(
            order.Id.ToString(), 
            new OrderCreatedIntegrationEvent(order.Id, order.TotalAmount, DateTime.UtcNow)
        );
        return order.Id;
    }
}
