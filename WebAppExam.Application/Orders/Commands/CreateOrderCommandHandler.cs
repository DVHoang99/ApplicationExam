using System.Reflection.Metadata;
using KafkaFlow.Producers;
using MediatR;
using WebAppExam.Application.Orders.Events;
using WebAppExam.Domain;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Orders.Commands;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Ulid>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IProducerAccessor _producerAccessor;


    public CreateOrderCommandHandler(
        ICustomerRepository customerRepository,
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IProducerAccessor producerAccessor)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _producerAccessor = producerAccessor;
        _customerRepository = customerRepository;
    }

    public async Task<Ulid> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        var customerExists = await _customerRepository.GetByIdAsync(request.CustomerId, ct);

        if (customerExists == null)
        {
            var failure = new FluentValidation.Results.ValidationFailure("Customer", "Customer not found.");
            throw new FluentValidation.ValidationException(new[] { failure });
        }

        var order = new Order(request.CustomerId, request.Address, request.Address, request.PhoneNumber);

        var products = await _productRepository.GetProductByIdsAsync(request.Items.Select(x => x.ProductId).ToList(), ct);

        foreach (var item in request.Items)
        {
            if (!products.ContainsKey(item.ProductId))
            {
                var failure = new FluentValidation.Results.ValidationFailure("Product", $"ProductId {item.ProductId} not found.");
                throw new FluentValidation.ValidationException(new[] { failure });
            }

            var product = products[item.ProductId];

            var inventory = product.Inventories.FirstOrDefault(x => x.Id == item.InventoryId);

            var inventoryId = inventory == null ? item.InventoryId : inventory.Id;

            order.AddOrUpdateItem(item.ProductId, product.Price, item.Quantity, inventoryId);
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
