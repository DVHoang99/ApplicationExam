using MediatR;
using WebAppExam.Application.Orders.Commands;
using WebAppExam.Domain;
using WebAppExam.Domain.Repository;
using WebAppExam.Infrastructure.UnitOfWork;

namespace WebAppExam.Application.Orders.Handlers;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, Ulid>
{
    private readonly IOrderRepository _orderRepo;
    private readonly IProductRepository _productRepo;
    private readonly IUnitOfWork _uow;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepo,
        IProductRepository productRepo,
        IUnitOfWork uow)
    {
        _orderRepo = orderRepo;
        _productRepo = productRepo;
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
            var product = await _productRepo.GetByIdAsync(item.ProductId);

            order.Details.Add(new OrderDetail
            {
                ProductId = product.Id,
                Quantity = item.Quantity,
                Discount = item.Discount
            });

            //await _inventory.DecreaseStock(product.Id, item.Quantity);
        }
        

        await _orderRepo.AddAsync(order);

        await _uow.SaveChangesAsync();

        return order.Id;
    }
}