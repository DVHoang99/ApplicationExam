using MassTransit;

namespace WebAppExam.Domain
{
    public class OrderProductMap : EntityBase
    {
        public string ProductId { get; set; }
        public string OrderId { get; set; }
        public int Quantity { get; set; }
    }
}
