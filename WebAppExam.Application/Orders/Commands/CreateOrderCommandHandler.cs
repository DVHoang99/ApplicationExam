using System.Reflection.Metadata;
using System.Text.Json;
using Confluent.Kafka;
using KafkaFlow;
using KafkaFlow.Producers;
using MediatR;
using WebAppExam.Application.Common.Caching;
using WebAppExam.Application.Orders.DTOs;
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
        IProducerAccessor producerAccessor
        )
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _customerRepository = customerRepository;
        _producerAccessor = producerAccessor;
    }

    // public async Task<Ulid> Handle(CreateOrderCommand request, CancellationToken ct)
    // {
    //     var customerExists = await _customerRepository.GetByIdAsync(request.CustomerId, ct);

    //     if (customerExists == null)
    //     {
    //         var failure = new FluentValidation.Results.ValidationFailure("Customer", "Customer not found.");
    //         throw new FluentValidation.ValidationException(new[] { failure });
    //     }

    //     var order = new Order(request.CustomerId, request.Address, request.CustomerName, request.PhoneNumber);

    //     var products = await _productRepository.GetProductByIdsAsync(request.Items.Select(x => x.ProductId).ToList(), ct);

    //     foreach (var item in request.Items)
    //     {
    //         if (!products.ContainsKey(item.ProductId))
    //         {
    //             var failure = new FluentValidation.Results.ValidationFailure("Product", $"ProductId {item.ProductId} not found.");
    //             throw new FluentValidation.ValidationException(new[] { failure });
    //         }

    //         var product = products[item.ProductId];

    //         //var inventory = product.Inventories.FirstOrDefault(x => x.Id == item.InventoryId);

    //         //var inventoryId = inventory == null ? item.InventoryId : inventory.Id;

    //         //order.AddOrUpdateItem(item.ProductId, product.Price, item.Quantity, inventoryId);
    //         //product.AddOrUpdateInventory(inventoryId, -item.Quantity, product.Id, inventory.Name);
    //     }

    //     await _orderRepository.AddAsync(order, ct);

    //     _productRepository.UpdateRange(products.Values.ToList());

    //     var producer = _producerAccessor.GetProducer("order-events-producer");

    //     await producer.ProduceAsync(
    //         order.Id.ToString(),
    //         new OrderCreatedIntegrationEvent(order.Id, order.TotalAmount, DateTime.UtcNow, 1)
    //     );
    //     return order.Id;
    // }

    public async Task<Ulid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        if (request.Items == null || !request.Items.Any())
            throw new Exception("Giỏ hàng đang trống, không thể tạo đơn!");

        // 1. GỘP SẢN PHẨM TRÙNG LẶP
        // Đề phòng UI lỗi gửi lên 2 dòng trùng ProductId và WareHouseId
        var groupedItems = request.Items
            .GroupBy(x => new { x.ProductId, x.WareHouseId })
            .Select(g => new OrderItemDto
            {
                ProductId = g.Key.ProductId,
                WareHouseId = g.Key.WareHouseId,
                Quantity = g.Sum(x => x.Quantity)
            })
            .ToList();

        var orderId = Ulid.NewUlid();

        // 2. LƯU ORDER VÀO DATABASE VỚI TRẠNG THÁI "PENDING"
        var newOrder = new Order(request.CustomerId, request.Address, request.CustomerName, request.PhoneNumber);

        // Lưu xuống Postgres (hoặc SQL Server)
        await _orderRepository.AddAsync(newOrder, cancellationToken);

        var orderCreatedEvent = new OrderCreatedEvent
        {
            OrderId = orderId.ToString(),
            CustomerName = request.CustomerName,
            Items = request.Items.Select(x => new OrderItemEvent
            {
                ProductId = x.ProductId.ToString(),
                Quantity = x.Quantity,
                WareHouseId = x.WareHouseId.ToString()
            }).ToList()
        };

        // 4. BẮN MESSAGE QUA KAFKA FLOW
        // Dùng OrderId làm Key để Kafka băm (Hash) vào Partition, đảm bảo thứ tự

        var producer = _producerAccessor.GetProducer("order-producer");

        await producer.ProduceAsync(
            orderId.ToString(),
            orderCreatedEvent
        );

        return orderId;
    }
}
