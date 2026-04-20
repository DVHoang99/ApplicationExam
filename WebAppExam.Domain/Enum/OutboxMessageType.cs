using System.ComponentModel;

namespace WebAppExam.Domain.Enum;

public enum OutboxMessageType
{
    [Description("Order Created")]
    OrderCreated = 1,
    [Description("Order Updated")]
    OrderUpdated = 2,
    [Description("Order Deleted")]
    OrderDeleted = 3,
    [Description("Order Cancelled")]
    OrderCancelled = 4,

}

public static class OutboxMessageTypeExtensions
{
    public static string Description(this OutboxMessageType outboxMessageType)
    {
        var type = outboxMessageType.GetType();
        var memInfo = type.GetMember(outboxMessageType.ToString());
        var attributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
        return ((DescriptionAttribute)attributes[0]).Description;
    }
}
