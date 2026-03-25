using MediatR;
using Microsoft.EntityFrameworkCore;
using WebAppExam.Domain.ViewModels;
using WebAppExam.Infra;

namespace WebAppExam.Application.Inventory.Queries.GetProductByIdQuery
{
    public class GetProductByIdQuery(Guid id) : IRequest<GetInventoryViewModel>
    {
        public Guid Id { get; } = id;
    }

    public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, GetInventoryViewModel>
    {
        private readonly ApplicationDbContext _context;

        public GetProductByIdHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<GetInventoryViewModel> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.Id);

            if (product == null)
            {
                throw new KeyNotFoundException("Product not found");
            }

            return new GetInventoryViewModel
            {
                Id = product.Id.ToString(),
                Name = product.Name,
                Price = product.Price
            };
        }
    }
}
