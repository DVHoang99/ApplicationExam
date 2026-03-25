namespace WebAppExam.Domain
{
    public class Order : EntityBase
    {
        public Ulid CustomerId { get; set; } = Ulid.Empty;
        public OrderStatus Status { get; set; }
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
