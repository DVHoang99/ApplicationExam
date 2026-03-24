using MediatR;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson.Serialization.IdGenerators;
using WebAppExam.Domain;
using WebAppExam.Infra;
using WebAppExam.Infra.Services;

namespace WebAppExam.Application.Order.Commands.AddProductToOrderCommand
{
    public class AddProductToOrderCommand(string orderId, string productId, int quantity) : IRequest<string>
    {
        public string OrderId { get; } = orderId;
        public string ProductId { get; } = productId;
        public int Quantity { get; } = quantity;
    }
    public class AddProductToOrderCommandHandler : IRequestHandler<AddProductToOrderCommand, string>
    {
        private readonly ApplicationDbContext _context;
        private readonly ICacheLockService _lockService;

        public AddProductToOrderCommandHandler(
            ApplicationDbContext context,
            ICacheLockService lockService)
        {
            _context = context;
            _lockService = lockService;
        }
        public async Task<string> Handle(AddProductToOrderCommand request, CancellationToken cancellationToken)
        {
            string lockKey = $"lock:product:{request.ProductId}";

            bool success = await _lockService.ExecuteWithLockAsync(lockKey, TimeSpan.FromSeconds(10));

            if (!success)
            {
                throw new Exception("System is busy, please try again later.");
            }

            return await AddProductToCartAsync(request.OrderId, request.ProductId, request.Quantity, cancellationToken);
        }

        public async Task<string> AddProductToCartAsync(string orderId, string productId, int quantity, CancellationToken cancellationToken)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id.ToString() == orderId, cancellationToken);

            if (order == null)
            {
                throw new Exception("order not found");
            }

            var product = await _context.Products.FirstOrDefaultAsync(product => product.Id.ToString() == productId, cancellationToken);

            if (product == null)
            {
                throw new Exception("Product not found");
            }

            var orderProductMap = await _context.OrderProductMaps.FirstOrDefaultAsync(map => map.OrderId == orderId && map.ProductId == productId, cancellationToken);

            if (orderProductMap == null)
            {
                var map = new OrderProductMap
                {
                    OrderId = orderId,
                    ProductId = productId,
                    Quantity = quantity,
                };

                await _context.OrderProductMaps.AddAsync(map, cancellationToken);
            }
            else 
            {
                orderProductMap.Quantity += quantity;

                if (orderProductMap.Quantity <= 0)
                { 
                    _context.OrderProductMaps.Remove(orderProductMap);
                }
            }

            product.Quantity -= quantity;
            await _context.SaveChangesAsync();

            return orderId;
        }
    }
}
