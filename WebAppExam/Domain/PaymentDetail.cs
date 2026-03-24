namespace WebAppExam.Domain
{
    public class PaymentDetail : EntityBase
    {
        public Guid OrderId { get; set; }
        public Order Order { get; set; }
    }
}
