using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebAppExam.Domain;
using WebAppExam.Infra;

namespace WebAppExam.Application.Order.Commands.CreateOrderCommand
{
    public class CreateOrderCommand(string customerId) : IRequest<string>
    {
        public string CustomerId { get; set; } = customerId;
    }

    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, string>
    {
        private readonly ApplicationDbContext _context;

        public CreateOrderCommandHandler(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
           var order = new Domain.Order { CustomerId = request.CustomerId };

            await _context.Orders.AddAsync(order, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return order.Id.ToString();
        }
    }
}
