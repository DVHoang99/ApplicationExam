using System;

namespace WebAppExam.Domain.Common;

public static class Constants
{
    public static class KafkaPrefix
    {
        public const string OrderCreatedPrefix = "order-created";
        public const string OrderUpdatePrefix = "order-update";
    }
}
