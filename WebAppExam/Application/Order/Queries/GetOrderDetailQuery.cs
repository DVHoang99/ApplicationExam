using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebAppExam.Domain.ViewModels;
using WebAppExam.Infra;

namespace WebAppExam.Application.Order.Queries
{
    public class GetOrderDetailQuery(Guid orderId) : IRequest<OrderDetailViewModel>
    {
        public Guid OrderId { get; } = orderId;
    }

    public class GetOrderDetailQueryHandler : IRequestHandler<GetOrderDetailQuery, OrderDetailViewModel>
    {
        private readonly ApplicationDbContext _context;
        public GetOrderDetailQueryHandler(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
        }
        public async Task<OrderDetailViewModel> Handle(GetOrderDetailQuery request, CancellationToken cancellationToken)
        {
            var order = await _context.Orders.FindAsync(request.OrderId, cancellationToken);

            if (order == null)
            {
                throw new KeyNotFoundException($"Order with ID {request.OrderId} not found.");
            }

            var customer = await _context.Customers.FindAsync(order.CustomerId, cancellationToken);

            var orderProductMaps = await _context
                .OrderProductMaps
                .Where(opm => opm.OrderId == order.Id.ToString())
                .ToListAsync(cancellationToken);

            var productIds = orderProductMaps.Select(opm => opm.ProductId).ToList();

            var products = await _context.Products
                .Where(p => productIds.Contains(p.Id.ToString()))
                .ToDictionaryAsync(d => d.Id, d => d);

            return new OrderDetailViewModel
            {
                OrderId = order.Id.ToString(),
                Customer = customer == null ? new CustomerViewModel() : new CustomerViewModel
                {
                    Id = customer.Id.ToString(),
                    CustomerName = customer.CustomerName,
                    Email = customer.Email
                },
                Products = orderProductMaps.Select(opm => new ProductViewModel
                {
                    Id = opm.ProductId,
                    Name = products.ContainsKey(Guid.Parse(opm.ProductId)) ? products[Guid.Parse(opm.ProductId)].Name : "",
                    Price = products.ContainsKey(Guid.Parse(opm.ProductId)) ? products[Guid.Parse(opm.ProductId)].Price : 0
                }).ToList(),
                total = orderProductMaps.Sum(opm => products.ContainsKey(Guid.Parse(opm.ProductId)) ? products[Guid.Parse(opm.ProductId)].Price : 0)
            };
        }
    }
}
