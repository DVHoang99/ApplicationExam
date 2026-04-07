using System;
using FluentResults;
using WebAppExam.Application.Products.DTOs;

namespace WebAppExam.Application.Products.Services;

public interface IProductService
{
    Task<Result<Ulid>> CreateProductAsync(string name, string? description, int price, string wareHouseId, int stock, CancellationToken cancellationToken = default);
    Task<Result<List<ProductDTO>>> GetAllProductsAsync(string name, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<Result<ProductDTO>> GetProductByIdAsync(Ulid id, CancellationToken cancellationToken = default);
    Task<Result<Ulid>> UpdateProductAsync(Ulid id, string name, string? description, int price, string wareHouseId, int stock, CancellationToken cancellationToken = default);
    Task<Result<Ulid>> DeleteProductAsync(Ulid id, string wareHouseId, CancellationToken cancellationToken = default);
}
