using System;

namespace WebAppExam.Domain.Events;

public class OrderItemEvent
{
    public string ProductId { get; set; }
    public int Quantity { get; set; }
    public string WareHouseId { get; set; }
}
