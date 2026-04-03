using MediatR;
using WebAppExam.Application.Common;
using WebAppExam.Application.Orders.DTOs;
using WebAppExam.Application.Orders.Events;
using WebAppExam.Domain;
using WebAppExam.Domain.Events;
using WebAppExam.Domain.Repository;
namespace WebAppExam.Application.Orders.Commands;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Ulid>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IInventoryReservationService _inventoryReservationService;

    public CreateOrderCommandHandler(
        ICustomerRepository customerRepository,
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IInventoryReservationService inventoryReservationService)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _customerRepository = customerRepository;
        _inventoryReservationService = inventoryReservationService;
    }

    public async Task<Ulid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        if (request.Items == null || !request.Items.Any())
            throw new Exception("Cart is empty, cannot create order!");

        var customerExists = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customerExists == null)
            throw new FluentValidation.ValidationException(new[] { new FluentValidation.Results.ValidationFailure("Customer", "Customer not found.") });

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

        var products = await _productRepository.GetProductByIdsAndWareHouseIdsAsync(
            groupedItems.Select(x => x.ProductId).ToList(),
            groupedItems.Select(x => x.WareHouseId).ToList(),
            cancellationToken);

        var newOrder = new Order(request.CustomerId, request.Address, request.CustomerName, request.PhoneNumber);

        foreach (var item in groupedItems)
        {
            if (!products.TryGetValue(item.ProductId, out var product))
            {
                var failure = new FluentValidation.Results.ValidationFailure("Product", $"ProductId {item.ProductId} not found.");
                throw new FluentValidation.ValidationException(new[] { failure });
            }

            newOrder.AddOrUpdateItem(item.ProductId, product.Price, item.Quantity, Ulid.Parse(item.WareHouseId));
        }

        await _inventoryReservationService.ReserveStocksAsync(request.CustomerId, groupedItems);

        try
        {

            await _orderRepository.AddAsync(newOrder, cancellationToken);

            var orderCreatedEvent = new OrderCreatedEvent
            {
                OrderId = newOrder.Id.ToString(),
                CustomerName = request.CustomerName,
                Items = groupedItems.Select(x => new OrderItemEvent
                {
                    ProductId = x.ProductId.ToString(),
                    Quantity = x.Quantity,
                    WareHouseId = x.WareHouseId.ToString()
                }).ToList()
            };

            newOrder.AddEventDomain(orderCreatedEvent);

            return newOrder.Id;
        }
        catch (Exception ex)
        {
            await _inventoryReservationService.ReleaseStocksAsync(groupedItems);
            throw new Exception("Lỗi khi lưu đơn hàng, đã hoàn trả lại tồn kho.", ex);
        }
    }
}