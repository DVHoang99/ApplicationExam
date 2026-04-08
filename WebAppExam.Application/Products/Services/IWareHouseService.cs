using System;
using WebAppExam.Application.Products.DTOs;

namespace WebAppExam.Application.Products.Services;

public interface IWareHouseService
{
    Task<WareHouseDTO?> GetWareHouseAsync(string wareHouseId, CancellationToken cancellationToken = default);
    Task<WareHouseDTO?> GetWareHouseGrpcAsync(string wareHouseId, CancellationToken cancellationToken = default);
}
