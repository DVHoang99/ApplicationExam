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

    public static class CachePrefix
    {
        public const string OrderDetailPrefix = "order-detail";
        public const string CustomerDetailPrefix = "customer-detail";
        public static string InventoriesStock(string wareHouseId, string productId)
        {
            return $"inventory:stock:{wareHouseId}:{productId}";
        }
    }

    public static class HangfireJob
    {
        public const string DailyRevenueCalculation = "daily-revenue-calculation";
    }
}
