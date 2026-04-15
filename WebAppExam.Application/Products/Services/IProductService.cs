using System;
using FluentResults;
using WebAppExam.Application.Products.DTOs;

namespace WebAppExam.Application.Products.Services;

/// <summary>
/// Defines the contract for product-related operations.
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="name">The name of the product.</param>
    /// <param name="description">A description of the product.</param>
    /// <param name="price">The price of the product.</param>
    /// <param name="wareHouseId">The identifier of the warehouse where the product is stored.</param>
    /// <param name="stock">The initial stock quantity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The unique identifier of the created product.</returns>
    Task<Result<Ulid>> CreateProductAsync(string name, string? description, int price, string wareHouseId, int stock, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated list of products based on filters.
    /// </summary>
    /// <param name="name">The product name to filter by.</param>
    /// <param name="pageNumber">The page number for pagination.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of product data transfer objects.</returns>
    Task<Result<List<ProductDTO>>> GetAllProductsAsync(string name, int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a specific product by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the product.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The product data transfer object.</returns>
    Task<Result<ProductDTO>> GetProductByIdAsync(Ulid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing product's information.
    /// </summary>
    /// <param name="id">The unique identifier of the product to update.</param>
    /// <param name="name">The updated name of the product.</param>
    /// <param name="description">The updated description of the product.</param>
    /// <param name="price">The updated price.</param>
    /// <param name="wareHouseId">The identifier of the warehouse.</param>
    /// <param name="stock">The updated stock quantity.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The unique identifier of the updated product.</returns>
    Task<Result<Ulid>> UpdateProductAsync(Ulid id, string name, string? description, int price, string wareHouseId, int stock, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a product.
    /// </summary>
    /// <param name="id">The unique identifier of the product to delete.</param>
    /// <param name="wareHouseId">The identifier of the warehouse from which to remove the product.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The unique identifier of the deleted product.</returns>
    Task<Result<Ulid>> DeleteProductAsync(Ulid id, string wareHouseId, CancellationToken cancellationToken = default);
}
