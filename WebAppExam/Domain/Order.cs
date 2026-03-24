namespace WebAppExam.Domain
{
    public class Order : EntityBase
    {
        public string CustomerId { get; set; } = string.Empty;
        public OrderStatus Status { get; set; }
        public Guid PaymentId { get; set; }
        public Customer Customer { get; set; }
        public PaymentDetail PaymentDetail { get; set; }
    }

    public enum OrderStatus
    { 
        Cancel = 0,
        Pending = 1,
        WaitingForPayment = 2,
        Paid = 3,
    }
}
