using System;

namespace WebAppExam.Application.Services;

public interface IInventoryReconciliationJob
{
    Task ReconcilePendingProductsAsync();
}
