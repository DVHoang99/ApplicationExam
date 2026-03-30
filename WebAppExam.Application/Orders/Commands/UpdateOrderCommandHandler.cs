using System;
using MediatR;
using WebAppExam.Application.Common.Caching;
using WebAppExam.Domain.Repository;

namespace WebAppExam.Application.Orders.Commands;

public class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand, Ulid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICacheService _cacheService;

    public UpdateOrderCommandHandler(IOrderRepository orderRepository,
    ICacheService cacheService,
    IProductRepository productRepository)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _cacheService = cacheService;
    }

    public async Task<Ulid> Handle(UpdateOrderCommand request, CancellationToken ct)
    {
        var order = await _orderRepository.GetByIdAsync(request.Id, ct);
        if (order == null)
            throw new Exception("Order not found");

        order.UpdateOrderGeneralInformation(request.CustomerId, request.CustomerName, request.Address, request.PhoneNumber);

        order.CustomerId = request.CustomerId;
        order.Address = request.Address;
        order.PhoneNumber = request.PhoneNumber;

        var products = await _productRepository.GetProductByIdsAsync(request.Items.Select(x => x.ProductId).ToList(), ct);

        foreach (var item in request.Items)
        {
            if (!products.ContainsKey(item.ProductId))
                continue;

            var product = products[item.ProductId];

            var inventory = product.Inventories.FirstOrDefault(x => x.Id == item.InventoryId);

            var inventoryId = inventory == null ? item.InventoryId : inventory.Id;


            order.AddOrUpdateItem(item.ProductId, product.Price, item.Quantity, inventoryId);
        }

        _orderRepository.Update(order);

        await _cacheService.RemoveByPrefixAsync($"order_detail:{request.Id}");
        return order.Id;
    }
}
