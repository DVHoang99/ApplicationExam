using MediatR;
using Microsoft.EntityFrameworkCore;
using WebAppExam.Infra;

namespace WebAppExam.Application.Inventory.Queries.GetInventoryQuery
{
    public class GetInventoryQuery(string productName, int pageNumber = 1, int pageSize = 20) : IRequest<GetinventoryViewModelWapper>
    {
        public string ProductName { get; } = productName;
        public int PageNumber { get; } = pageNumber;
        public int PageSize { get; } = pageSize;
    }

    public class GetInventoryQueryHandler : IRequestHandler<GetInventoryQuery, GetinventoryViewModelWapper>
    {
        private readonly ApplicationDbContext _context;

        public GetInventoryQueryHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<GetinventoryViewModelWapper> Handle(GetInventoryQuery request, CancellationToken cancellationToken)
        {
            var cursor = _context.Products
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.ProductName))
            {
                cursor = cursor.Where(p => EF.Functions.ILike(p.Name, $"{request.ProductName}%"));
            }

            var total = await cursor.CountAsync(cancellationToken);

            var products = await cursor
                .OrderByDescending(p => p.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            return new GetinventoryViewModelWapper
            {
                Total = total,
                Data = products.Select(GetInventoryViewModel.FromResult).ToList(),
            };
        }
    }
}
