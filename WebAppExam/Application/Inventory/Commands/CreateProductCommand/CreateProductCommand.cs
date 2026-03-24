using Hangfire;
using MassTransit;
using MediatR;
using WebAppExam.Application.Inventory.Command.CreateProductCommand;
using WebAppExam.Domain;
using WebAppExam.Infra;

namespace WebAppExam.Application.Inventory.Command.CreateProductCommand
{
    public class CreateProductCommand(string name, int price, string? description, int quantity) : IRequest<string>
    {
        public string Name { get; } = name;
        public int Price { get; } = price;
        public string? Description { get; } = description;
        public int Quantity { get; } = quantity;
    }
}
public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, string>
{
    private readonly ApplicationDbContext _context;
    //private readonly ITopicProducer<string, ApplicationLog> _logProducer;

    public CreateProductCommandHandler(
        ApplicationDbContext context
        //ITopicProducer<string, ApplicationLog> logProducer
        )
    {
        _context = context;
        //_logProducer = logProducer;
    }

    public async Task<string> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product
        {
            Name = request.Name,
            Price = request.Price,
            Description = request.Description ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            Quantity = request.Quantity,
        };

        _context.Products.Add(product);

        await _context.SaveChangesAsync(cancellationToken);
        //await _logProducer.Produce(nameof(CreateProductCommand), new ApplicationLog
        //{
        //    Level = "Success",
        //    Message = "Data processing complete",
        //    Source = "OrderService"
        //});

        return product.Id.ToString();
    }
}
