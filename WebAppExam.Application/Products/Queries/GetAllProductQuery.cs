using System;
using MediatR;
using WebAppExam.Application.Products.DTOs;

namespace WebAppExam.Application.Products.Queries;

public class GetAllProductQuery(string searchTerm, int pageNumber, int pageSize) : IRequest<List<ProductDTO>>
{
    public string SearchTerm { get; private set; } = searchTerm;
    public int PageNumber { get; private set; } = pageNumber;
    public int PageSize { get; private set; } = pageSize;

    public static GetAllProductQuery Init(string searchTerm, int pageNumber, int pageSize)
    {
        return new GetAllProductQuery(searchTerm, pageNumber, pageSize);
    }
}
