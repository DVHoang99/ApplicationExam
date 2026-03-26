using System;
using MediatR;
using WebAppExam.Application.Products.DTOs;

namespace WebAppExam.Application.Products.Queries;

public class GetAllProductQuery(string searchTerm) : IRequest<List<ProductDTO>>
{
    public string SearchTerm { get; set; } = searchTerm;
}
