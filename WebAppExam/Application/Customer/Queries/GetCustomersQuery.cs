using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebAppExam.Domain.ViewModels;
using WebAppExam.Infra;

namespace WebAppExam.Application.Customer.Queries
{
    public class GetCustomersQuery(
    string? PhoneNumber,
    string? CustomerName
    ) : IRequest<List<CustomerViewModel>>
    {
        public string? PhoneNumber { get; } = PhoneNumber;
        public string? CustomerName { get; } = CustomerName;
    }

    public class GetCustomersHandler : IRequestHandler<GetCustomersQuery, List<CustomerViewModel>>
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetCustomersHandler(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<CustomerViewModel>> Handle(GetCustomersQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Customers.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                query = query.Where(x => x.PhoneNumber.Contains(request.PhoneNumber));
            }

            if (!string.IsNullOrWhiteSpace(request.CustomerName))
            {
                query = query.Where(x => x.CustomerName.Contains(request.CustomerName));
            }

            return await query
                .ProjectTo<CustomerViewModel>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}
