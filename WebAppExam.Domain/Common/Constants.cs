using System;

namespace WebAppExam.Domain.Common;

public static class Constants
{
    public static class KafkaPrefix
    {
        public const string OrderCreatedPrefix = "order-created";
        public const string OrderUpdatePrefix = "order-update";
        public const string OrderDeletedPrefix = "order-deleted";
        public const string OrderCanceledPrefix = "order-canceled";
    }

    public static class CachePrefix
    {
        public const string OrderDetailPrefix = "order-detail";
        public const string CustomerDetailPrefix = "customer-detail";
        public static string InventoriesStock(string wareHouseId, string productId)
        {
            return $"inventory:stock:{wareHouseId}:{productId}";
        }
    }
}
