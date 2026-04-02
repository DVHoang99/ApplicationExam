using MediatR;
using WebAppExam.Application.Common.Caching;
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
    private readonly ICacheLockService _lockService;


    public CreateOrderCommandHandler(
        ICustomerRepository customerRepository,
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        ICacheLockService lockService
        )
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _customerRepository = customerRepository;
        _lockService = lockService;
    }

    public async Task<Ulid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var customerExists = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);

        if (customerExists == null)
        {
            var failure = new FluentValidation.Results.ValidationFailure("Customer", "Customer not found.");
            throw new FluentValidation.ValidationException(new[] { failure });
        }

        if (request.Items == null || !request.Items.Any())
            throw new Exception("Giỏ hàng đang trống, không thể tạo đơn!");

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
            var newOrder = new Order(request.CustomerId, request.Address, request.CustomerName, request.PhoneNumber);
            var products = await _productRepository.GetProductByIdsAndWareHouseIdsAsync(
            request.Items.Select(x => x.ProductId).ToList(),
            request.Items.Select(x => x.WareHouseId).ToList(),
            cancellationToken);

            var invalidProducts = request.Items.Where(x => !products.ContainsKey(x.ProductId));

            if (invalidProducts.Any())
            {
                var failure = new FluentValidation.Results.ValidationFailure("Product", "Product not found.");
                throw new FluentValidation.ValidationException(new[] { failure });
            }

            foreach (var item in request.Items)
            {
                if (!products.ContainsKey(item.ProductId))
                {
                    var failure = new FluentValidation.Results.ValidationFailure("Product", $"ProductId {item.ProductId} not found.");
                    throw new FluentValidation.ValidationException(new[] { failure });
                }

                var product = products[item.ProductId];

                newOrder.AddOrUpdateItem(item.ProductId, product.Price, item.Quantity, Ulid.Parse(item.WareHouseId));
            }

            await _orderRepository.AddAsync(newOrder, cancellationToken);

            var orderCreatedEvent = new OrderCreatedEvent
            {
                OrderId = newOrder.Id.ToString(),
                CustomerName = request.CustomerName,
                Items = request.Items.Select(x => new OrderItemEvent
                {
                    ProductId = x.ProductId.ToString(),
                    Quantity = x.Quantity,
                    WareHouseId = x.WareHouseId.ToString()
                }).ToList()
            };

            newOrder.AddEventDomain(orderCreatedEvent);

            return newOrder.Id;
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
