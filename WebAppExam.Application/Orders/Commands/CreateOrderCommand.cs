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
        var order = new Order
        {
            CustomerId = request.CustomerId,
            Status = OrderStatus.Pending
        };

        decimal total = 0;
        int qty = 0;

        foreach (var item in request.Items)
        {
            var product = await _uow.Products.GetByIdAsync(item.ProductId);

            order.Details.Add(new OrderDetail
            {
                ProductId = product.Id,
                Quantity = item.Quantity,
                Discount = item.Discount
            });
        }


        await _uow.Orders.AddAsync(order, ct);

        await _uow.SaveChangesAsync();

        return order.Id;
    }
}