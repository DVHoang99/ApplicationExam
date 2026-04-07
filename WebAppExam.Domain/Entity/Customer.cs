namespace WebAppExam.Domain
{
    public class Customer : EntityBase
    {
        public string CustomerName { get; private set; }
        public string PhoneNumber { get; private set; }
        public string Email { get; private set; }

        private Customer(string customerName, string email, string phoneNumber)
        {
            Id = Ulid.NewUlid();
            CustomerName = customerName;
            Email = email;
            PhoneNumber = phoneNumber;
        }

        public static Customer Create(string customerName, string email, string phone)
        {
            return new Customer(customerName, email, phone);
        }

        public void Update(string customerName, string email, string phone)
        {
            CustomerName = customerName;
            Email = email;
            PhoneNumber = phone;
        }

        public void Delete()
        {
            DeletedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
