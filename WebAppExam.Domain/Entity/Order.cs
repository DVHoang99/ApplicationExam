using WebAppExam.Domain.Enum;

namespace WebAppExam.Domain
{
    public class Order : EntityBase, IAggregateRoot
    {
        public Ulid CustomerId { get; private set; }
        public int TotalAmount { get; private set; }
        public OrderStatus Status { get; private set; }
        public string Address { get; private set; }
        public string CustomerName { get; private set; }
        public string PhoneNumber { get; private set; }
        public string Reason { get; private set; } = string.Empty;

        private readonly List<OrderDetail> _details = new();
        public IReadOnlyCollection<OrderDetail> Details => _details.AsReadOnly();

        private Order(Ulid customerId, string address, string customerName, string phoneNumber)
        {
            Id = Ulid.NewUlid();
            CustomerId = customerId;
            CreatedAt = DateTime.UtcNow;
            Status = OrderStatus.Draft;
            Address = address;
            CustomerName = customerName;
            PhoneNumber = phoneNumber;
        }

        public static Order Init(Ulid customerId, string address, string customerName, string phoneNumber)
        {
            return new Order(customerId, address, customerName, phoneNumber);
        }

        public OrderDetail AddOrUpdateItem(Ulid productId, int unitPrice, int quantity, Ulid wereHouseId)
        {
            OrderDetail affectedItem;
            var existingItem = _details.SingleOrDefault(x => x.ProductId == productId && x.WareHouseId == wereHouseId);

            if (existingItem != null)
            {
                var oldQuantity = existingItem.Quantity;
                existingItem.UpdateQuantity(quantity);
                affectedItem = new OrderDetail(productId, unitPrice, existingItem.Quantity - oldQuantity, wereHouseId);
            }
            else
            {
                _details.Add(new OrderDetail(productId, unitPrice, quantity, wereHouseId));
                affectedItem = new OrderDetail(productId, unitPrice, quantity, wereHouseId);
            }
            RecalculateTotal();

            return affectedItem;
        }

        public OrderDetail? RemoveItem(Ulid productId, string wareHouseId)
        {
            var parsedWareHouseId = Ulid.Parse(wareHouseId);

            var item = _details.SingleOrDefault(x => x.ProductId == productId && x.WareHouseId == parsedWareHouseId);

            if (item == null)
            {
                return null;
            }
            var affectedItem = new OrderDetail(productId, item.Price, -item.Quantity, parsedWareHouseId);
            _details.Remove(item);
            RecalculateTotal();
            return affectedItem;
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
        public void UpdateOrderStatus(OrderStatus status, string reason)
        {
            Status = status;
            UpdatedAt = DateTime.UtcNow;
            Reason = reason;
        }
    }
}
