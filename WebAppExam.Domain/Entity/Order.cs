using WebAppExam.Domain.Enum;

namespace WebAppExam.Domain
{
    public class Order : EntityBase, IAggregateRoot
    {
        public Ulid CustomerId { get; set; }
        public int TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        private readonly List<OrderDetail> _details = new();
        public IReadOnlyCollection<OrderDetail> Details => _details.AsReadOnly();

        protected Order() { }

        public Order(Ulid customerId)
        {
            Id = Ulid.NewUlid();
            CustomerId = customerId;
            CreatedAt = DateTime.UtcNow;
            Status = OrderStatus.Pending;
        }

        public void AddOrUpdateItem(Ulid productId, int unitPrice, int quantity)
        {
            var existingItem = _details.SingleOrDefault(x => x.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.UpdateQuantity(quantity);
            }
            else
            {
                _details.Add(new OrderDetail(productId, unitPrice, quantity));
            }
        }

        public void RemoveItem(Ulid productId)
        {
            var item = _details.SingleOrDefault(x => x.ProductId == productId);
            if (item != null)
            {
                _details.Remove(item);
            }
        }
        private void RecalculateTotal()
        {
            TotalAmount = _details.Sum(x => x.Price * x.Quantity);
        }
    }
}
