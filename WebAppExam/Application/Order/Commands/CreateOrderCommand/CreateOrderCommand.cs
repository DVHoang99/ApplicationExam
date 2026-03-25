using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebAppExam.Domain;
using WebAppExam.Infra;

namespace WebAppExam.Application.Order.Commands.CreateOrderCommand
{
    public class CreateOrderCommand(Guid customerId) : IRequest<string>
    {
        public Guid CustomerId { get; set; } = customerId;
    }

    public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, string>
    {
        private readonly ApplicationDbContext _context;
        private readonly MassTransit.ITopicProducer<WebAppExam.Application.Events.OrderCreatedEvent> _producer;
        private readonly ILogger<CreateOrderCommandHandler> _logger;

        public CreateOrderCommandHandler(
            ApplicationDbContext context,
            MassTransit.ITopicProducer<WebAppExam.Application.Events.OrderCreatedEvent> producer,
            ILogger<CreateOrderCommandHandler> logger)
        {
            _context = context;
            _producer = producer;
            _logger = logger;
        }

        public async Task<string> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
        {
            // ensure customer exists to satisfy FK constraint
            var customer = await _context.Customers.FindAsync(new object[] { request.CustomerId }, cancellationToken);
            if (customer == null)
            {
                throw new KeyNotFoundException("Customer not found");
            }

            var order = new Domain.Order { CustomerId = request.CustomerId };

            await _context.Orders.AddAsync(order, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // publish event that order was created
            var evt = new WebAppExam.Application.Events.OrderCreatedEvent(order.Id, order.CustomerId, DateTime.UtcNow);
            try
            {
                _logger.LogInformation("Producing OrderCreatedEvent for OrderId={OrderId}", order.Id);
                await _producer.Produce(evt, cancellationToken);
                _logger.LogInformation("Produced OrderCreatedEvent for OrderId={OrderId}", order.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to produce OrderCreatedEvent for OrderId={OrderId}", order.Id);
                throw;
            }

            return order.Id.ToString();
        }
    }
}
