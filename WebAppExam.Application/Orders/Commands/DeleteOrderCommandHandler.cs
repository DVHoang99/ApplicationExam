using System;
using MediatR;
using WebAppExam.Application.Common.Caching;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Orders.Commands;

public class DeleteOrderCommandHandler : IRequestHandler<DeleteOrderCommand, Ulid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cacheService;


    public DeleteOrderCommandHandler(IOrderRepository orderRepository, ICacheService cacheService, IProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _cacheService = cacheService;
        _productRepository = productRepository;
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
            var inventory = product.Inventories.FirstOrDefault(x => x.Id == item.InventoryId);
            product.AddOrUpdateInventory(item.InventoryId, item.Quantity, product.Id, inventory.Name);
        }

        _orderRepository.Update(order);
        _productRepository.UpdateRange(products.Values.ToList());

        await _cacheService.RemoveByPrefixAsync($"order_detail:{request.Id}");
        return order.Id;
    }
}
