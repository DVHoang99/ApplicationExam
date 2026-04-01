using System;
using MediatR;
using WebAppExam.Application.Products.DTOs;

namespace WebAppExam.Application.Products.Queries;

public class GetAllProductQuery(string searchTerm, int pageNumber, int pageSize) : IRequest<List<ProductDTO>>
{
    public string SearchTerm { get; set; } = searchTerm;
    public int pageNumber { get; set; } = pageNumber;
    public int pageSize { get; set; } = pageSize;

}
