using WebAppExam.Domain.Enum;

namespace WebAppExam.Domain
{
    public class Order : EntityBase, IAggregateRoot
    {
        public Ulid CustomerId { get; set; }
        public int TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public string Address { get; set; }
        public string CustomerName { get; set; }
        public string PhoneNumber { get; set; }
        private readonly List<OrderDetail> _details = new();
        public IReadOnlyCollection<OrderDetail> Details => _details.AsReadOnly();

        protected Order() { }

        public Order(Ulid customerId, string address, string customerName, string phoneNumber)
        {
            Id = Ulid.NewUlid();
            CustomerId = customerId;
            CreatedAt = DateTime.UtcNow;
            Status = OrderStatus.Pending;
            Address = address;
            CustomerName = customerName;
            PhoneNumber = phoneNumber;
        }

        public void AddOrUpdateItem(Ulid productId, int unitPrice, int quantity, Ulid inventoryId)
        {
            var existingItem = _details.SingleOrDefault(x => x.ProductId == productId && x.InventoryId == inventoryId);

            if (existingItem != null)
            {
                existingItem.UpdateQuantity(quantity);
            }
            else
            {
                _details.Add(new OrderDetail(productId, unitPrice, quantity, inventoryId));
            }
            RecalculateTotal();
        }

        public void RemoveItem(Ulid productId)
        {
            var item = _details.SingleOrDefault(x => x.ProductId == productId);
            if (item != null)
            {
                _details.Remove(item);
            }
            RecalculateTotal();
        }
        private void RecalculateTotal()
        {
            TotalAmount = _details.Sum(x => x.Price * x.Quantity);
        }
        public void UpdateOrderGeneralInformation(Ulid customerId, string address, string customerName, string phoneNumber)
        {
            Address = address;
            CustomerName = customerName;
            PhoneNumber = phoneNumber;
            CustomerId = customerId;
        }

        public void DeleteOrder()
        {
            DeletedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
