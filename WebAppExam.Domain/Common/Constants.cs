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

    public static class KafkaTopic
    {
        public const string OrderCreatedTopic = "order-created-topic";
        public const string OrderUpdatedTopic = "order-updated-topic";
        public const string OrderDeletedTopic = "order-deleted-topic";
        public const string OrderCanceledTopic = "order-canceled-topic";
        public const string OrderTopic = "order-topic1";
        public const string OrderReplyTopic = "order-reply-topic";
        public const string OrderEventsTopic = "order-events";
        public const string SystemLogsTopic = "system-logs-topic";
    }

    public static class KafkaProducer
    {
        public const string OrderProducer = "order-producer";
        public const string OrderEventProducer = "order-events-producer";
        public const string SystemLogsProducer = "system-logs-producer";
    }

    public static class KafkaGroup
    {
        public const string OrderReplyTopicGroup = "order-reply-topic-group";
        public const string RevenueUpdateGroup = "revenue-update-group";
        public const string APIInternalLoggerGroup = "api-internal-logger-group";
    }

    public static class KafkaRetry
    {
        public const int DefaultRetryCount = 3;
        public static readonly TimeSpan InfrastructureRetryDelay = TimeSpan.FromMinutes(5);
    }

    public static class CachePrefix
    {
        public const string OrderDetailPrefix = "order-detail";
        public const string CustomerDetailPrefix = "customer-detail";
        public const string ProductDetailPrefix = "product_details";
        public const string HangfirePrefix = "hangfire:ecommerce:";
        
        public static string InventoriesStock(string wareHouseId, string productId)
        {
            return $"inventory:stock:{wareHouseId}:{productId}";
        }
    }

    public static class CacheDuration
    {
        public static readonly TimeSpan DefaultL1 = TimeSpan.FromMinutes(5);
        public static readonly TimeSpan DefaultL2 = TimeSpan.FromHours(2);
        public static readonly TimeSpan ProductDetail = TimeSpan.FromMinutes(10);
        public static readonly TimeSpan OrderDetail = TimeSpan.FromDays(1);
        public static readonly TimeSpan CustomerDetail = TimeSpan.FromDays(1);
        public static readonly TimeSpan Jitter = TimeSpan.FromSeconds(30);
    }

    public static class HangfireJob
    {
        public const string DailyRevenueCalculation = "daily-revenue-calculation";
    }

    public static class ConfigKeys
    {
        public const string RedisCacheDb = "Redis:CacheDb";
        public const string RedisSessionDb = "Redis:SessionDb";
        public const string RedisJobQueueDb = "Redis:JobQueueDb";
        public const string RedisInboxDb = "Redis:InboxDb";
        public const string RedisHangfireDb = "Redis:HangfireDb";
        public const string GrpcInventoryService = "GrpcService:InventoryService";
        public const string InternalInventoryService = "InternalService:InventoryService";
    }

    public static class ConfigDefaults
    {
        public const string LocalRedis = "localhost:6379";
        public const string LocalHangfireRedis = "localhost:6379,password=adminpassword,defaultDatabase=4";
        public const string LocalGrpc = "http://localhost:5000/";
        public const string LocalInventoryService = "http://localhost:5134/";
    }

    public static class HttpHeader
    {
        public const string Accept = "Accept";
        public const string ContentType = "Content-Type";
        public const string ApplicationJson = "application/json";
    }
}
