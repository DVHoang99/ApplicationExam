using MediatR;
using WebAppExam.Application.Orders.DTOs;
using WebAppExam.Domain;
using WebAppExam.Infrastructure.UnitOfWork;

namespace WebAppExam.Application.Orders.Commands;

public class CreateOrderCommand : IRequest<Ulid>
{
    public Ulid CustomerId { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Ulid>
{
    private readonly IUnitOfWork _uow;

    public CreateOrderCommandHandler(
        IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Ulid> Handle(CreateOrderCommand request, CancellationToken ct)
    {
        var customerExists = await _uow.Customers.GetByIdAsync(request.CustomerId, ct);

        if (customerExists == null)
            throw new KeyNotFoundException("Customer not found.");

        var order = new Order(request.CustomerId);

        var products = await _uow.Products.GetProductByIdsAsync(request.Items.Select(x => x.ProductId).ToList(), ct);

        foreach (var item in request.Items)
        {
            if (!products.ContainsKey(item.ProductId))
                continue;

            var product = products[item.ProductId];

            var price = product.Inventories.FirstOrDefault() == null
                ? 0
                : product.Inventories.FirstOrDefault().Price;

            order.AddOrUpdateItem(item.ProductId, price, item.Quantity);
        }

        await _uow.Orders.AddAsync(order, ct);

        await _uow.SaveChangesAsync();

        return order.Id;
    }
}