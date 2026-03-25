namespace WebAppExam.Domain
{
    public class Order : EntityBase
    {
        public Guid CustomerId { get; set; } = Guid.Empty;
        public OrderStatus Status { get; set; }
        public Guid PaymentId { get; set; }
        public Customer Customer { get; set; }
    }

    public enum OrderStatus
    { 
        Cancel = 0,
        Pending = 1,
        WaitingForPayment = 2,
        Paid = 3,
    }
}
