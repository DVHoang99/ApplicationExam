namespace WebAppExam.Domain
{
    public class Customer : EntityBase
    {
        public string CustomerName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }

        public List<Order> Orders { get; set; } = new();
    }
}
